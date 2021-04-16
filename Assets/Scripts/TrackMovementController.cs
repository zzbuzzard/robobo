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

    public override float WheelPower { get; set; }
    private const float baseWheelPower = 400.0f;

    private float movePowerMultiplier = 0.0f, turnPowerMultiplier = 0.0f;

    [SerializeField]
    private float dampConst = 0.5f; // 2 is perfect critical damping, lower is a faster but wobblier turn

    //[SerializeField]
    //private float moveForce = 1500.0f;

    //[SerializeField]
    //private float turnForce = 3500.0f;

    public TrackMovementController(RobotScript parent) : base(parent)
    {
        H = new List<XY>();
        V = new List<XY>();
    }

    public override void UpdateWheels(List<Block> newList)
    {
        H.Clear();
        V.Clear();

        for (int i=0; i<newList.Count; i++)
        {
            int rotation = Mathf.RoundToInt(newList[i].transform.localRotation.eulerAngles.z / 90.0f);
            XY pos = new XY(newList[i].x, newList[i].y);

            if (rotation % 2 == 0)
                H.Add(pos);
            else
                V.Add(pos);
        }

        LoadStats();
    }

    public override void Move(Vector2 moveDirection, Vector2 lookDirection, bool isLooking)
    {
        // WORLD -> LOCAL
        moveDirection = parent.transform.InverseTransformDirection(moveDirection);

        // If one is empty, we only move in forward direction

        // Vertical only
        if (H.Count == 0)
        {
            Vector2 forward = new Vector2(0, 1);
            moveDirection = Vector2.Dot(forward, moveDirection) * forward.normalized;
        }
        else
        if (V.Count == 0)
        {
            Vector2 forward = new Vector2(1, 0);
            moveDirection = Vector2.Dot(forward, moveDirection) * forward.normalized;
        }

        // Movement
        // TODO: maxMovePower should depend on direction; take x and y components, add vectors for max from each
        parent.mrig.AddRelativeForce(moveDirection * movePowerMultiplier * WheelPower);

        if (isLooking)
        {
            float ang = Vector2.SignedAngle(new Vector2(1, 0), lookDirection) / 360.0f;
            if (ang < 0) ang += 1;

            float curAng = parent.transform.rotation.eulerAngles.z / 360.0f - front;
            if (curAng < 0) curAng += 1;

            float turn = GetRotation(curAng, ang, parent.mrig);
            turn = CalculateTorque(turn);

            parent.mrig.AddTorque(turn);
        }
    }

    private float CalculateTorque(float angle)
    {
        float maxTurnPower = turnPowerMultiplier * WheelPower;

        float c = dampConst * Mathf.Sqrt(SMOA * maxTurnPower);
        float dampingMoment = c * parent.mrig.angularVelocity * Mathf.Deg2Rad;
        float springMoment = angle * maxTurnPower;

        return Mathf.Clamp(springMoment - dampingMoment, -maxTurnPower, maxTurnPower);
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

        movePowerMultiplier = H.Count + V.Count;
        turnPowerMultiplier = 0.0f;

        Vector2 maxWheelForce = new Vector2(1.0f, 0.0f);
        foreach (XY xy in H)
        {
            Vector2 localPos = XYToLocal(xy);
            Vector2 comToPos = localPos - com;

            turnPowerMultiplier += Mathf.Abs(Vector3.Cross(comToPos, maxWheelForce).z);
        }

        maxWheelForce = new Vector2(0.0f, 1.0f);
        foreach (XY xy in V)
        {
            Vector2 localPos = XYToLocal(xy);
            Vector2 comToPos = localPos - com;

            turnPowerMultiplier += Mathf.Abs(Vector3.Cross(comToPos, maxWheelForce).z);
        }

        movePowerMultiplier *= baseWheelPower;
        turnPowerMultiplier *= baseWheelPower;

        turnPowerMultiplier *= 2.5f;
    }
}
