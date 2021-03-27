using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonScript : WeaponBlock, IUsableBlock, ICollisionForwardParent
{
    public override BlockType Type => BlockType.PISTON;

    [SerializeField]
    private float damage = 0;

    [SerializeField]
    private float force = 10000f;
    private Animation anim;

    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animation>();
    }

    public void Use()
    {
        anim.Play();
    }

    public void ChildCollisionStay(Collision2D collision)
    {
    }

    // (Piston head collision)
    public void ChildCollision(Collision2D collision)
    {
        if (!anim.isPlaying) return;

        // Not necessarily damageable - physics only
        Block b = collision.collider.transform.GetComponent<Block>();

        // Don't hit myself, but hit any other physics object
        if (b != null && b.GetParent() == parent) return;

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

        collision.rigidbody.AddForceAtPosition(-avg_normal.normalized * force, avg_pos);
    }
}
