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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Must use collider to disambiguate child block and parent rigidbody
        Damageable d = collision.collider.transform.GetComponent<Damageable>();
        if (d == null || d.IsNull() || d.GetParent() == parent) return;

        WeaponBlock.SpeedDamage(collision, d, damage, this);
        WeaponBlock.StandardForce(collision, force);
    }
}
