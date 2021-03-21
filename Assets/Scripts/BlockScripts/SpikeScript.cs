using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Should inherit from some kinda WeaponBlock class
public class SpikeScript : Block
{
    public GameObject sparks;

    // TODO: Probably get rid of this nonsense and call TakeDamage() or whatever on collided block
    float force = 5000.0f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Don't do sparks if it's nobody or if it's me
        if (collision.rigidbody == null) return;
        if (collision.transform.parent == null || collision.transform.parent == transform.parent) return;

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
    }
}
