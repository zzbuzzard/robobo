using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeScript : WeaponBlock
{
    public override BlockType Type => BlockType.SPIKE;

    [SerializeField]
    private float force = 5000.0f;

    [SerializeField]
    private float damage = 5f;

    [SerializeField]
    private float damage_mul = 1;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Must use collider to disambiguate child block and parent rigidbody
        Damageable d = collision.collider.transform.GetComponent<Damageable>();
        if (d == null || d.IsNull() || d.GetParent() == parent) return;

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

        float increased_damage = damage * damage_mul * collision.relativeVelocity.magnitude;
        SparkScript.CreateSparks(avg_pos, increased_damage);
        DealDamage(d, increased_damage);
    }
}
