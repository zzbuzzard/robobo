using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSpikes : MonoBehaviour
{
    [SerializeField]
    private GameObject sparks;

    [SerializeField]
    private float force = 5000.0f;

    [SerializeField]
    private float damage = 10f;

    [SerializeField]
    private float damage_mul = 1;
    // Start is called before the first frame update
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Must use collider to disambiguate child block and parent rigidbody
        Damageable d = collision.collider.transform.GetComponent<Damageable>();
        if (d == null || d.IsNull()) return;

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

        collision.rigidbody.AddForceAtPosition(-avg_normal.normalized * force, avg_pos);

        Instantiate(sparks, (Vector3)avg_pos + new Vector3(0, 0, -1), Quaternion.identity);

        float increased_damage = damage * damage_mul * collision.relativeVelocity.magnitude;
        DealDamage(d, increased_damage);
    }

    // Update is called once per frame
    public void DealDamage(Damageable target, float damage)
    {
        // TODO: Debuffs?
        target.TakeDamage(damage);
    }
}
