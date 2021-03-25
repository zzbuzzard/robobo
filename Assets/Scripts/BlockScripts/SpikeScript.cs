using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeScript : WeaponBlock
{
    public override BlockType Type => BlockType.SPIKE;

    public GameObject sparks;

    [SerializeField]
    private float force = 5000.0f;

    [SerializeField]
    private float damage = 5f;

    [SerializeField]
    private float damage_mul = 1;

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
        //Debug.Log(increased_damage);
        // TODO: Something better than fixed damage every time
        // MUST BE AT END or if b dies we get NPE (i learnt the hard way)
        DealDamage(b, increased_damage);
    }
}
