using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField]
    private Block block;

    [SerializeField]
    private float damageMultiplier = 1.0f; // 1 means all damage taken, 0 means no damage taken

    public RobotScript GetParent()
    {
        if (block == null) return null;
        return block.GetParent();
    }

    public bool IsNull()
    {
        return block == null;
    }

    public void TakeDamage(float damage)
    {
        if (block == null)
        {
            return;
        }
        block.TakeDamage(damage * damageMultiplier);
    }
}
