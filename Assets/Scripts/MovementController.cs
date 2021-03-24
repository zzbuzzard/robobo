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

    public abstract void UpdateWheels(List<XY> newList);
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
}
