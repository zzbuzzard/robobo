using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour, IDamageDealer
{
    public void DealDamage(Damageable target, float damage)
    {
        target.TakeDamage(damage);
    }
}
