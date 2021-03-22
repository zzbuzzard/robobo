using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A script class, but it's abstract so can never be used
// Its purpose is to define base behaviour for all blocks

// Enforce blocks have a collider
[RequireComponent(typeof(Collider2D))]
public abstract class Block : MonoBehaviour
{
    public int hp;
    public float density;
    protected Collider2D myCollider; // base class for box collider, polygon collider, etc.
    protected MovementScript parent; // the MovementScript of my parent

    public int x, y;

    private int maxHP;

    private void Start()
    {
        maxHP = hp;
        myCollider = GetComponent<Collider2D>();
        myCollider.density = density;
        parent = transform.parent.GetComponent<MovementScript>();
    }

    // Overriden by ControlBlock
    public virtual bool IsControl()
    {
        return false;
    }

    // I have no idea why this is necessary. C# sucks
    // If you are a SpikeScript, and you have a Block b, then you can't access b.parent even though it's protected...
    public MovementScript GetParent()
    {
        return parent;
    }

    public void TakeDamage(int damage)
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
        DestroyBlock();
    }

    // TODO: Some kinda particle effect?
    // HP HIT ZERO: Tell parent
    private void Die()
    {
        // make sure everything only dies once
        if (dead) return;
        dead = true;

        // detach and tell parent
        DestroyBlock();
        parent.RemoveBlock(this);
    }

    private void DestroyBlock()
    {
        // Just for fun, so it can be kicked about
        Rigidbody2D rig = gameObject.AddComponent<Rigidbody2D>();
        rig.gravityScale = 0.0f;
        rig.mass = density;
        rig.drag = 0.5f;

        transform.SetParent(null);
        Destroy(this); // destroy this component
    }
}
