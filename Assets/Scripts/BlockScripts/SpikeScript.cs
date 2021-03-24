using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Should inherit from some kinda WeaponBlock class
public class SpikeScript : WeaponBlock
{
    public GameObject sparks;

    // TODO: This is all scuffed, needs some engineering attention ARNAV
    //       Also, damage done should maybe depend on force of collision or something?

    float force = 5000.0f;
    public float damage = 5f;
    public float damage_mul = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Damageable d = collision.collider.transform.GetComponent<Damageable>();
        if (d == null) return;
        Block b = d.block;

        // Don't do sparks if it's nobody or if it's part of the same parent
        if (b == null || b.GetParent() == parent) return;

        // Current method: Apply force at avg contact position, opposite to avg normal
        //  It's ok, but could do with a variable force using e.g. Contact.normalImpulse or whatever
        Vector2 avg_pos = Vector2.zero;
        Vector2 avg_normal = Vector2.zero;
        for (int i=0; i<collision.contactCount; i++)
        {
            avg_normal += collision.GetContact(i).normal;
            avg_pos += collision.GetContact(i).point;
        }
        avg_pos /= collision.contactCount;
        avg_normal /= collision.contactCount;

        collision.rigidbody.AddForceAtPosition(-avg_normal.normalized * force, avg_pos);

        Instantiate(sparks, (Vector3)avg_pos + new Vector3(0, 0, -1), Quaternion.identity);


        float increased_damage = damage * damage_mul * collision.relativeVelocity.magnitude;
        Debug.Log(increased_damage);
        // TODO: Something better than fixed damage every time
        // MUST BE AT END or if b dies we get NPE (i learnt the hard way)
        DealDamage(b, increased_damage);
    }

    public override void DealDamage(Block target, float damage)
    {
        target.TakeDamage(damage);
    }
}
