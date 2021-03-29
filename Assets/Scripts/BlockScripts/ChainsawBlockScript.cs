using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainsawBlockScript : WeaponBlock, IUsableBlock, ICollisionForwardParent
{
    public override BlockType Type => BlockType.CHAINSAW;

    private CapsuleCollider2D capsule;

    [SerializeField]
    private float force = 10000f;

    [SerializeField]
    private float damage = 5f;

    private bool isRunning = false;
    private Animator anim;

    protected override void Start()
    {
        base.Start();
        capsule = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
    }

    public void Use()
    {
        isRunning = !isRunning;
        anim.SetBool("IsRunning", isRunning);
    }

    protected override void HandleDeath()
    {
        base.HandleDeath();
        isRunning = false;
        anim.SetBool("IsRunning", false);
    }

    // Handle collision here- the child is the blade.
    public void ChildCollisionStay(Collision2D collision)
    {
        if (!isRunning) return;
        //if (collision.otherCollider != capsule) return;

        // Must use collider to disambiguate child block and parent rigidbody
        Damageable d = collision.collider.transform.GetComponent<Damageable>();
        if (d == null || d.IsNull() || d.GetParent() == parent) return;

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

        // Add force
        collision.rigidbody.AddForceAtPosition(Vector2.Perpendicular(-avg_normal.normalized) * force, avg_pos);

        float dealDamage = damage * Time.fixedDeltaTime;

        SparkScript.CreateSparks(avg_pos, dealDamage);

        Vector2 worldPos = transform.TransformPoint(avg_pos);
        Vector2 worldForce = transform.TransformDirection(Vector2.Perpendicular(-avg_normal.normalized) * force);
        Debug.DrawLine(worldPos, worldPos + worldForce * 0.01f);

        // TODO: Better damage dealing?
        DealDamage(d, dealDamage);
    }

    public void ChildCollision(Collision2D collision)
    {

    }
}
