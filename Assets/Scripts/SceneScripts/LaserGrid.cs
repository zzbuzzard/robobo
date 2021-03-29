using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGrid : Hazard
{
    [SerializeField]
    private float damage = 10f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Must use collider to disambiguate child block and parent rigidbody
        Damageable d = collision.collider.transform.GetComponent<Damageable>();
        if (d == null || d.IsNull()) return;

        WeaponBlock.FixedDamage(collision, d, damage, this);
    }
}
