using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

public class TrackMovementController : MovementController
{
    private float front = 0.75f;

    public List<XY> H;
    public List<XY> V;

    //private float MASS;
    private float SMOA;

    public float dampConst = 0.5f; // 2 is perfect critical damping, lower is a faster but wobblier turn
    public float moveForce = 1500.0f;
    public float turnForce = 3500.0f;

    public TrackMovementController(RobotScript parent) : base(parent)
    {
        H = new List<XY>();
        V = new List<XY>();
    }

    public override void UpdateWheels(List<XY> newList, List<int> rotation)
    {
        H.Clear();
        V.Clear();

        for (int i=0; i<newList.Count; i++)
        {
            if (rotation[i] % 2 == 0)
                H.Add(newList[i]);
            else
                V.Add(newList[i]);
        }

        LoadStats(); 
    }

    public override void Move(Vector2 moveDirection, Vector2 lookDirection)
    {
        // WORLD -> LOCAL
        moveDirection = parent.transform.InverseTransformDirection(moveDirection);

        // If one is empty, we only move in forward direction
        Vector2 forward = new Vector2(0, 1);
        if (H.Count * V.Count == 0)
            moveDirection = Vector2.Dot(forward, moveDirection) * forward.normalized;

        ApplyMovement(moveDirection);

        float ang = Vector2.SignedAngle(new Vector2(1, 0), lookDirection) / 360.0f;
        if (ang < 0) ang += 1;

        float curAng = parent.transform.rotation.eulerAngles.z / 360.0f - front;
        if (curAng < 0) curAng += 1;

        // get the two cases for rotation
        float a, b;
        if (curAng < ang)
        {
            a = ang - curAng;
            b = ang - 1 - curAng;
        }
        else
        {
            a = ang + 1 - curAng;
            b = ang - curAng;
        }
        float turn;
        if (Mathf.Abs(a) < Mathf.Abs(b)) turn = a;
        else turn = b;

        turn = CalculateTorque(turn);
        ApplyTorque(turn);
    }

    private void ApplyTorque(float f)
    {
        parent.mrig.AddTorque(f);
    }

    // F being a local force
    private void ApplyMovement(Vector2 f)
    {
        parent.mrig.AddRelativeForce(f * moveForce);
    }

    private float CalculateTorque(float angle)
    {
        float maxTurn = turnForce * (H.Count + V.Count);

        float c = dampConst * Mathf.Sqrt(SMOA * maxTurn);
        float dampingMoment = c * parent.mrig.angularVelocity * Mathf.Deg2Rad;
        float springMoment = angle * maxTurn;

        return Mathf.Clamp(springMoment - dampingMoment, -maxTurn, maxTurn);
    }

    // TODO: I scuffed arnavs calculation (in my defence, you don't seem to be able to do collider.area)
    private void LoadStats()
    {
        SMOA = 0;
        Vector2 com = parent.mrig.centerOfMass;
        for (int i = 0; i < parent.children.Count; i++)
        {
            Collider2D r = parent.children[i].GetComponent<Collider2D>();
            Vector2 this_com = parent.children[i].transform.localPosition;
            SMOA += r.density * Mathf.Pow((this_com - com).magnitude, 2.0f);
            // smoa += r.mass * Mathf.Pow((r.worldCenterOfMass - COM).magnitude, 2.0f);
        }
    }
}
