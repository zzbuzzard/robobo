using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainsawBlockScript : UsableWeaponBlock
{
    public override BlockType Type => BlockType.CHAINSAW;

    public GameObject sparks;
    private CapsuleCollider2D capsule;
    public float force = 10000f;
    public float damage = 5f;
    private bool isRunning = false;
    private Animator anim;

    protected override void Start()
    {
        base.Start();
        capsule = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
    }

    public override void Use()
    {
        isRunning = !isRunning;
        anim.SetBool("IsRunning", isRunning);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isRunning) return;
        if (collision.otherCollider != capsule) return;

        Block b = collision.collider.transform.GetComponent<Block>();

        // Don't do sparks if it's nobody or if it's part of the same parent
        if (b == null || b.GetParent() == parent) return;

        // If it's not a physics object, return
        if (collision.rigidbody == null) return;

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

    protected override void HandleDeath()
    {
        base.HandleDeath();
        isRunning = false;
        anim.SetBool("IsRunning", false);
    }
}
