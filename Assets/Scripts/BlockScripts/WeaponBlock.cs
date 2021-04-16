using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBlock : Block, IDamageDealer
{
    public override WheelType Wheel => WheelType.NONE;

    public void DealDamage(Damageable target, float damage)
    {
        // TODO: Debuffs?
        target.TakeDamage(damage);
    }


    //////// Utils
    private static Vector2 AveragePos(Collision2D collision)
    {
        Vector2 avg_pos = Vector2.zero;
        for (int i = 0; i < collision.contactCount; i++)
            avg_pos += collision.GetContact(i).point;
        return avg_pos / collision.contactCount;
    }

    private static Vector2 AverageNormal(Collision2D collision)
    {
        Vector2 avg_normal = Vector2.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            avg_normal += collision.GetContact(i).normal;
        }
        return avg_normal / collision.contactCount;
    }

    // Apply a force opposite to the average normal
    public static void StandardForce(Collision2D collision, float force)
    {
        Vector2 avgPos = AveragePos(collision);
        Vector2 avgNormal = AverageNormal(collision);

        collision.rigidbody.AddForceAtPosition(-avgNormal.normalized * force, avgPos);
    }

    // Applies in direction direction
    public static void DirectedForce(Collision2D collision, float force, Vector2 direction)
    {
        Vector2 avgPos = AveragePos(collision);

        collision.rigidbody.AddForceAtPosition(direction.normalized * force, avgPos);
    }

    public static void ChainsawForce(Collision2D collision, float force, Transform caller)
    {
        Vector2 avgPos = AveragePos(collision);
        Vector2 avgNormal = AverageNormal(collision);

        // Add force
        collision.rigidbody.AddForceAtPosition(Vector2.Perpendicular(-avgNormal.normalized) * force, avgPos);

        Vector2 worldPos = caller.TransformPoint(avgPos);
        Vector2 worldForce = caller.TransformDirection(Vector2.Perpendicular(-avgNormal.normalized) * force);
        //Debug.DrawLine(worldPos, worldPos + worldForce * 0.01f);
    }

    // Apply damage proportional to collision speed
    public static void SpeedDamage(Collision2D collision, Damageable d, float damage, IDamageDealer dealer)
    {
        Vector2 avgPos = AveragePos(collision);
        float increased_damage = damage * collision.relativeVelocity.magnitude;

#if UNITY_SERVER
#else
        SparkScript.CreateSparks(avgPos, increased_damage);
#endif

        dealer.DealDamage(d, increased_damage);
    }

    // Apply damage proportional to collision speed
    public static void FixedDamage(Collision2D collision, Damageable d, float damage, IDamageDealer dealer)
    {
        Vector2 avgPos = AveragePos(collision);

        float increased_damage = damage * collision.relativeVelocity.magnitude;

#if UNITY_SERVER
#else
        SparkScript.CreateSparks(avgPos, increased_damage);
#endif

        dealer.DealDamage(d, increased_damage);
    }
}
