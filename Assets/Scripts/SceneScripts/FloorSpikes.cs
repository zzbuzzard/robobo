using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSpikes : Hazard
{
    [SerializeField]
    private float force = 5000.0f;

    [SerializeField]
    private float damage = 10f;
    
    // Start is called before the first frame update
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Must use collider to disambiguate child block and parent rigidbody
        Damageable d = collision.collider.transform.GetComponent<Damageable>();
        if (d == null || d.IsNull()) return;

        WeaponBlock.SpeedDamage(collision, d, damage, this);
        WeaponBlock.StandardForce(collision, force);
    }
}
