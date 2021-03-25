using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

public class HoverMovementController : MovementController
{
    private float front = 0.75f;
    
    private List<Block> wheels;
    
    private float SMOA;

    private float maxWheelPower = 750.0f;
    public float dampConst = 0.5f; // 2 is perfect critical damping, lower is a faster but wobblier turn

    private float maxMovePower = 0.0f, maxTurnPower = 0.0f;

    public HoverMovementController(RobotScript parent) : base(parent)
    {
    }

    public override void UpdateWheels(List<Block> newList)
    {
        wheels = newList;
        LoadStats(); 
    }

    public override void Move(Vector2 moveDirection, Vector2 lookDirection)
    {
        // WORLD -> LOCAL
        moveDirection = parent.transform.InverseTransformDirection(moveDirection);

        // Movement
        parent.mrig.AddRelativeForce(moveDirection * maxMovePower);

        float ang = Vector2.SignedAngle(new Vector2(1, 0), lookDirection) / 360.0f;
        if (ang < 0) ang += 1;

        float curAng = parent.transform.rotation.eulerAngles.z / 360.0f - front;
        if (curAng < 0) curAng += 1;

        float turn = GetRotation(curAng, ang, parent.mrig);
        turn = CalculateTorque(turn);

        parent.mrig.AddTorque(turn);

        for (int i=0; i<wheels.Count; i++)
        {
            Vector2 comToWheel = (Vector2)wheels[i].transform.localPosition - parent.mrig.centerOfMass;
            Vector2 perp = Vector2.Perpendicular(comToWheel);

            ((HoverScript)wheels[i]).ShowForce(moveDirection * maxMovePower + perp.normalized * turn);
        }
    }

    private float CalculateTorque(float angle)
    {
        float c = dampConst * Mathf.Sqrt(SMOA * maxTurnPower);
        float dampingMoment = c * parent.mrig.angularVelocity * Mathf.Deg2Rad;
        float springMoment = angle * maxTurnPower;

        return Mathf.Clamp(springMoment - dampingMoment, -maxTurnPower, maxTurnPower);
    }

    // TODO: I scuffed the SMOA calculation (in my defence, you don't seem to be able to do collider.area)
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

        // Calculate max move and turn power
        maxMovePower = maxWheelPower * wheels.Count;
        maxTurnPower = 0.0f;
        foreach (Block b in wheels)
        {
            Vector2 localPos = b.transform.localPosition;
            Vector2 comToPos = localPos - com;

            maxTurnPower += comToPos.magnitude * maxWheelPower;
        }
        maxTurnPower *= 1.5f;
    }
}

/*
COMPLETE BACKUP OF THE OLD *BEAUTIFUL* CODE:
(I can't bring myself to just delete it... let it live in this comment...)

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
    private Vector2[] localForces;
    private List<Block> wheels;

    //private float MASS;
    private float SMOA;

    //private float maxWheelPower = 300.0f;
    public float dampConst = 0.5f; // 2 is perfect critical damping, lower is a faster but wobblier turn

    public float moveForce = 500;
    public float turnForce = 10000.0f; // eek this is a bit high

    public HoverMovementController(RobotScript parent) : base(parent)
    {
    }

    public override void UpdateWheels(List<Block> newList)
    {
        wheels = newList;
        LoadStats(); 
        LoadTurnOneUnit();
    }

    public override void Move(Vector2 moveDirection, Vector2 lookDirection)
    {
        for (int i=0; i<localForces.Length; i++)
        {
            localForces[i].x = localForces[i].y = 0;
        }

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

        for (int i=0; i<localForces.Length; i++)
        {
            ApplyForce(localForces[i], wheels[i].transform.localPosition);
            ((HoverScript)wheels[i]).ShowForce(localForces[i]);
        }
    }

    private void ApplyTorque(float f)
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            //Vector2 localPos = wheels[i].transform.localPosition;
            //ApplyForce(turnOneUnit[i] * f, localPos);

            localForces[i] += turnOneUnit[i] * f;
        }
    }

    // F being a local force
    private float ApplyMovement(Vector2 f)
    {
        f *= moveForce;

        float moment = 0;
        for (int i = 0; i < wheels.Count; i++)
        {
            Vector2 localPos = wheels[i].transform.localPosition;
            //ApplyForce(f, localPos);
            localForces[i] += f;

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

        // TODO: Use this if we go for non-per-hover approach
        //maxMovePower = maxWheelPower * wheels.Count;
        //maxTurnPower = 0.0f;
        //foreach (Block b in wheels)
        //{
        //    Vector2 localPos = b.transform.localPosition;
        //    Vector2 comToPos = localPos - com;

        //    maxTurnPower += comToPos.magnitude * maxWheelPower;
        //}
    }

    private void LoadTurnOneUnit()
    {
        int N = wheels.Count;
        if (N == 0) return;

        turnOneUnit = new Vector2[N];
        localForces = new Vector2[N];

        Vector2 totForce = Vector2.zero;
        Vector2 com = parent.mrig.centerOfMass;

        for (int i = 0; i < N - 1; i++)
        {
            Vector2 localPos = wheels[i].transform.localPosition;
            Vector2 comToPos = localPos - com;
            Vector2 rotated = Vector2.Perpendicular(comToPos);
            turnOneUnit[i] = rotated;
            totForce += turnOneUnit[i];
        }

        float moment = 0;
        turnOneUnit[N - 1] = -totForce;
        for (int i = 0; i < N; i++)
        {
            Vector2 localPos = wheels[i].transform.localPosition;
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

*/
