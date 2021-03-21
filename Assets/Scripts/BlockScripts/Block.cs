using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A script class, but it's abstract so can never be used
// It's purpose is to define base behaviour for all blocks

// Enforce blocks have a collider
[RequireComponent(typeof(Collider2D))]
public abstract class Block : MonoBehaviour
{
    public int hp;
    public float density;
    protected Collider2D myCollider; // base class for box collider, polygon collider, etc.
    protected MovementScript parent; // the MovementScript of my parent

    // I have no idea why this is necessary. C# sucks
    // If you are a SpikeScript, and you have a Block b, then you can't access b.parent even though it's protected...
    public MovementScript GetParent()
    {
        return parent;
    }

    private void Start()
    {
        myCollider = GetComponent<Collider2D>();
        myCollider.density = density;
        parent = transform.parent.GetComponent<MovementScript>();
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

    // TODO: Some kinda particle effect?
    private void Die()
    {
        // detach and tell parent
        transform.SetParent(null);
        parent.BlocksChanged();
    }
}
