using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverScript : MovementBlock
{
    public override BlockType Type => BlockType.HOVER;
    public override WheelType Wheel => WheelType.HOVER;

    private float maxMoveRad = 0.15f;
    private float maxForceMove = 500.0f;
    public GameObject moveObj;

    public void ShowForce(Vector2 localForce)
    {
        localForce /= maxForceMove;
        if (localForce.magnitude > 1.0f)
            localForce = localForce.normalized;

        localForce *= maxMoveRad;

        moveObj.transform.localPosition = Vector2.Lerp(moveObj.transform.localPosition, localForce, 0.1f);
    }
}
