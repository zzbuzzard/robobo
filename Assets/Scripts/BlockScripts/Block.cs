using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A script class, but it's abstract so can never be used
// Its purpose is to define base behaviour for all blocks

// Enforce blocks have a collider
[RequireComponent(typeof(Collider2D))]
public abstract class Block : MonoBehaviour
{
    public abstract BlockType Type { get; }
    public abstract WheelType Wheel { get; }

    public float hp;
    public float density;
    protected Collider2D myCollider; // base class for box collider, polygon collider, etc.
    protected RobotScript parent; // the RobotScript of my parent

    public int x, y;

    private float maxHP;

    protected virtual void Start()
    {
        maxHP = hp;
        myCollider = GetComponent<Collider2D>();
        myCollider.density = density;
        parent = transform.parent.GetComponent<RobotScript>();
    }

    // I have no idea why this is necessary. C# sucks
    // If you are a SpikeScript, and you have a Block b, then you can't access b.parent even though it's protected...
    public RobotScript GetParent()
    {
        return parent;
    }

    public virtual void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
        ChangeHpDisplay();
    }

    // TODO: Show cracks etc
    private void ChangeHpDisplay()
    {

    }

    private bool dead = false;

    public bool IsDead() { return dead; }

    // SOMEONE ELSES' HP HIT ZERO: We were told by parent
    public void Detach()
    {
        // make sure everything only dies once
        if (dead) return;
        dead = true;

        // detach, but don't tell parent as we came from the parent
        HandleDeath();
        Destroy(this); // destroy this component
    }

    // TODO: Some kinda particle effect?
    // HP HIT ZERO: Tell parent
    private void Die()
    {
        // make sure everything only dies once
        if (dead) return;
        dead = true;

        // detach and tell parent
        HandleDeath();
        parent.RemoveBlock(this); // Must be last, as this statement may delete this object
        Destroy(this); // destroy this component
    }

    // It's hard sometimes, I know
    protected virtual void HandleDeath()
    {
        Rigidbody2D rig = gameObject.AddComponent<Rigidbody2D>();
        rig.gravityScale = 0.0f;
        rig.drag = 5;
        rig.angularDrag = 1;
        rig.mass = density * 1.5f * 1.5f; // should be collider area, but that doesn't seem to be gettable
        rig.velocity = parent.mrig.GetPointVelocity(transform.position);

        transform.SetParent(null);
    }
}
