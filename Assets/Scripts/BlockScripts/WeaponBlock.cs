using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBlock : Block
{
    public override WheelType Wheel => WheelType.NONE;

    public void DealDamage(Block target, float damage)
    {
        // TODO: Debuffs?
        target.TakeDamage(damage);
    }
}