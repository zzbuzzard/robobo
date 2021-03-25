using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

public abstract class MovementController
{
    protected RobotScript parent;
    public MovementController(RobotScript parent)
    {
        this.parent = parent;
    }

    public abstract void UpdateWheels(List<Block> newList);
    public abstract void Move(Vector2 moveDirection, Vector2 lookDirection);

    protected Vector2 XYToLocal(XY pos)
    {
        return (Vector2)pos * 1.5f;
    }

    protected Vector2 XYToWorld(XY pos)
    {
        return parent.transform.TransformPoint((Vector2)pos * 1.5f);
    }

    protected void ApplyForce(Vector2 localForce, Vector2 localPos)
    {
        Vector2 worldPos = parent.transform.TransformPoint(localPos);
        Vector2 worldForce = parent.transform.TransformDirection(localForce);
        parent.mrig.AddForceAtPosition(worldForce, worldPos);
        Debug.DrawLine(worldPos, worldPos + worldForce * 0.1f, Color.green);
    }
    protected void ApplyForceWorld(Vector2 worldForce, Vector2 worldPos)
    {
        parent.mrig.AddForceAtPosition(worldForce, worldPos);
    }

    public static float GetRotation(float currentAngle, float goalAngle)
    {
        // Try the two cases for rotation
        float a, b;
        if (currentAngle < goalAngle)
        {
            a = goalAngle - currentAngle;
            b = goalAngle - 1 - currentAngle;
        }
        else
        {
            a = goalAngle + 1 - currentAngle;
            b = goalAngle - currentAngle;
        }
        float turn;
        if (Mathf.Abs(a) < Mathf.Abs(b)) turn = a;
        else turn = b;

        return turn;
    }

    // TODO: Make a better choice about which way to turn, given rig.angularVelocity
    public static float GetRotation(float currentAngle, float goalAngle, Rigidbody2D rig)
    {
        return GetRotation(currentAngle, goalAngle);
    }
}
