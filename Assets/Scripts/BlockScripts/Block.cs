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

    // TODO: void Start should set collider density to density

    public void TakeDamage(int damage)
    {
        // TODO: Show damage / modify sprite etc

        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // dunno
    }
}
