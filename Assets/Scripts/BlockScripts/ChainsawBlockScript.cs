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

        WeaponBlock.FixedDamage(collision, d, damage * Time.fixedDeltaTime, this);
        WeaponBlock.ChainsawForce(collision, force, transform);
    }

    public void ChildCollision(Collision2D collision)
    {
    }
}
