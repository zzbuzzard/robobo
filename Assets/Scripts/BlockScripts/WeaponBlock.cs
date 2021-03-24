using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBlock : Block
{
    public abstract void DealDamage(Block target, float damage);
}
