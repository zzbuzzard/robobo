using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainsawBlockScript : UsableWeaponBlock
{
    public GameObject sparks;
    private CapsuleCollider2D capsule;
    public float force = 10000f;
    public float damage = 5f;

    private Animation anim;

    protected override void Start()
    {
        base.Start();
        capsule = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animation>();
    }


    // Start is called before the first frame update

    public override void Use()
    {
        Debug.Log("Tryna play");
        if (!anim.isPlaying)
        {
            Debug.Log("playing");
            anim.Play();
            return;
        }
        Debug.Log("Stopping");
        anim.Stop();

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!anim.isPlaying) return;
        if (collision.otherCollider != capsule) return;

        Block b = collision.collider.transform.GetComponent<Block>();

        // Don't do sparks if it's nobody or if it's part of the same parent
        if (b == null || b.GetParent() == parent) return;

        // Current method: Apply force at avg contact position, opposite to avg normal
        //  It's ok, but could do with a variable force using e.g. Contact.normalImpulse or whatever
        Vector2 avg_pos = Vector2.zero;
        Vector2 avg_normal = Vector2.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            avg_normal += collision.GetContact(i).normal;
            avg_pos += collision.GetContact(i).point;
        }
        avg_pos /= collision.contactCount;
        avg_normal /= collision.contactCount;

        collision.rigidbody.AddForceAtPosition(Vector2.Perpendicular(-avg_normal.normalized) * force, avg_pos);

        Instantiate(sparks, (Vector3)avg_pos + new Vector3(0, 0, -1), Quaternion.identity);
        Vector2 worldPos = transform.TransformPoint(avg_pos);
        Vector2 worldForce = transform.TransformDirection(Vector2.Perpendicular(-avg_normal.normalized) * force);
        Debug.DrawLine(worldPos, worldPos + worldForce * 0.01f);
        // TODO: Something better than fixed damage every time
        // MUST BE AT END or if b dies we get NPE (i learnt the hard way)
        DealDamage(b, damage);
    }

    public override void DealDamage(Block target, float damage)
    {
        target.TakeDamage(damage);
    }
}
