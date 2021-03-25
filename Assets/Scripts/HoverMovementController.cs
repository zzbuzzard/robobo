using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

// Movement:
// First, produce a set of forces which move in direction V
//  -> Set all wheels force to V
//  -> Calculate the produced turning moment, which we wish to cancel

// Second, produce a set of forces which rotate around COM without moving (resultant force is 0)
//  -> To do this, set all wheels force to perpendicular
//  -> Then, set the last wheel's direction to cancel this out.
//  -> Then, calculate turning moment and scale it up to be what's desired

// Idea; we store for each one, initial offset

public class HoverMovementController : MovementController
{
    // TODO: This variable needs a better home.
    // It would be cool to pick the front angle for your robot e.g. at 45 degrees if u were shaped like

        //         forward this way
        //        /
        //  o o o 
        //      o
        //      o

    private float front = 0.75f;

    private Vector2[] turnOneUnit; // Forces required to turn a unit about COM
    private List<XY> wheels;

    //private float MASS;
    private float SMOA;

    public float dampConst = 0.5f; // 2 is perfect critical damping, lower is a faster but wobblier turn
    public float moveForce = 500;
    public float turnForce = 10000.0f; // eek this is a bit high

    public HoverMovementController(RobotScript parent) : base(parent)
    {
    }

    public override void UpdateWheels(List<XY> newList, List<int> rotation)
    {
        wheels = newList;
        LoadStats(); 
        LoadTurnOneUnit();
    }

    public override void Move(Vector2 moveDirection, Vector2 lookDirection)
    {
        // WORLD -> LOCAL
        moveDirection = parent.transform.InverseTransformDirection(moveDirection);
        float cancel = ApplyMovement(moveDirection);

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

        ApplyTorque(turn - cancel);
    }

    private void ApplyTorque(float f)
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            Vector2 localPos = XYToLocal(wheels[i]);
            ApplyForce(turnOneUnit[i] * f, localPos);
        }
    }

    // F being a local force
    private float ApplyMovement(Vector2 f)
    {
        f *= moveForce;

        float moment = 0;
        for (int i = 0; i < wheels.Count; i++)
        {
            Vector2 localPos = XYToLocal(wheels[i]);
            ApplyForce(f, localPos);
            Vector2 comToPos = localPos - parent.mrig.centerOfMass;
            moment += Vector3.Cross(comToPos, f).z;
        }
        return moment;
    }

    private float CalculateTorque(float angle)
    {
        float maxTurn = turnForce * wheels.Count;

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

    private void LoadTurnOneUnit()
    {
        int N = wheels.Count;
        if (N == 0) return;

        turnOneUnit = new Vector2[N];

        Vector2 totForce = Vector2.zero;
        Vector2 com = parent.mrig.centerOfMass;

        for (int i = 0; i < N - 1; i++)
        {
            Vector2 localPos = XYToLocal(wheels[i]);
            Vector2 comToPos = localPos - com;
            Vector2 rotated = Vector2.Perpendicular(comToPos);
            turnOneUnit[i] = rotated;
            totForce += turnOneUnit[i];
        }

        float moment = 0;
        turnOneUnit[N - 1] = -totForce;
        for (int i = 0; i < N; i++)
        {
            Vector2 localPos = XYToLocal(wheels[i]);
            Vector2 comToPos = localPos - com;
            moment += Vector3.Cross(comToPos, turnOneUnit[i]).z;
        }

        // TODO: ... should probs do something about this?
        if (moment == 0)
        {
            Debug.LogWarning("CAN'T TURN");
            return;
        }

        for (int i = 0; i < N; i++)
        {
            turnOneUnit[i] *= 1.0f / moment;
        }
    }
}
