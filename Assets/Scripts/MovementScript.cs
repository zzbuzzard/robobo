using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Movement:
// First, produce a set of forces which move in direction V
//  -> Set all wheels force to V
//  -> Calculate the produced turning moment, which we wish to cancel

// Second, produce a set of forces which rotate around COM without moving (resultant force is 0)
//  -> To do this, set all wheels force to perpendicular
//  -> Then, set the last wheel's direction to cancel this out.
//  -> Then, calculate turning moment and scale it up to be what's desired

// Idea; we store for each one, initial offset

[RequireComponent(typeof(Rigidbody2D))]
public class MovementScript : MonoBehaviour
{
    public List<Vector2> wheelPositions;
    private Vector2[] wheelForces;
    private Vector2[] turnOneUnit;

    private List<GameObject> children;

    Rigidbody2D mrig;

    //private float MASS;
    private float SMOA;

    // 100, 5000
    private float dampConst = 0.5f; // 2 is perfect critical damping, lower is faster turn but wobblier
    private float moveForce = 500;
    private float turnForce = 10000.0f;

    // Front angle
    private float front = 0.75f;

    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();

        // Initialises turn forces etc
        BlocksChanged();
    }

    // e.g. lost a wheel/block
    // TODO: remove from children/wheel list
    public void BlocksChanged()
    {
        // Load children
        children = new List<GameObject>();
        int c = transform.childCount;
        for (int i = 0; i < c; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }

        wheelForces = new Vector2[wheelPositions.Count];
        turnOneUnit = new Vector2[wheelPositions.Count];

        LoadStats();
        LoadTurnOneUnit();
    }

    private void LoadStats()
    {
        SMOA = 0;
        Vector2 com = mrig.centerOfMass;
        for (int i = 0; i < children.Count; i++)
        {
            Collider2D r = children[i].GetComponent<Collider2D>();
            Vector2 this_com = children[i].transform.localPosition;
            SMOA += r.density * Mathf.Pow((this_com - com).magnitude, 2.0f);
            // smoa += r.mass * Mathf.Pow((r.worldCenterOfMass - COM).magnitude, 2.0f);
        }
    }

    private void LoadTurnOneUnit()
    {
        int N = wheelPositions.Count;
        Vector2 totForce = Vector2.zero;
        Vector2 com = mrig.centerOfMass;

        for (int i = 0; i < N - 1; i++)
        {
            Vector2 comToPos = wheelPositions[i] - com;
            Vector2 rotated = Vector2.Perpendicular(comToPos);
            turnOneUnit[i] = rotated;
            totForce += turnOneUnit[i];
        }

        float moment = 0;
        turnOneUnit[N - 1] = -totForce;
        for (int i = 0; i < N; i++)
        {
            Vector2 comToPos = wheelPositions[i] - com;
            moment += Vector3.Cross(comToPos, turnOneUnit[i]).z;
        }

        // MOMENT CANCELLED OUT: THEY CAN'T TURN
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

    private void ApplyTorque(float f)
    {
        for (int i = 0; i < wheelPositions.Count; i++)
        {
            wheelForces[i] += turnOneUnit[i] * f;
        }
    }

    // returns the moment produced
    // sets the force on every wheel to f
    private float ApplyMovement(Vector2 f)
    {
        float moment = 0;
        for (int i = 0; i < wheelPositions.Count; i++)
        {
            wheelForces[i] = f;
            Vector2 comToPos = wheelPositions[i] - mrig.centerOfMass;
            moment += Vector3.Cross(comToPos, f).z;
        }
        return moment;
    }

    private float CalculateTorque(float angle)
    {
        float c = dampConst * Mathf.Sqrt(SMOA * turnForce);
        float dampingMoment = c * mrig.angularVelocity * Mathf.Deg2Rad;
        float springMoment = angle * turnForce;

        return Mathf.Clamp(springMoment - dampingMoment, -turnForce, turnForce);
    }

    void ApplyForce(Vector2 localForce, Vector2 localPos)
    {
        Vector2 worldPos = transform.TransformPoint(localPos);
        Vector2 worldForce = transform.TransformDirection(localForce);
        mrig.AddForceAtPosition(worldForce, worldPos);
    }

    void FixedUpdate()
    {
        Vector2 movement = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            movement.x -= 1;
        if (Input.GetKey(KeyCode.D))
            movement.x += 1;
        if (Input.GetKey(KeyCode.W))
            movement.y += 1;
        if (Input.GetKey(KeyCode.S))
            movement.y -= 1;

        // world direction -> local direction
        movement = transform.InverseTransformDirection(movement);
        float cancelMoment = ApplyMovement(movement * moveForce);

        Vector2 worldGoal = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 comWorld = mrig.worldCenterOfMass;

        float ang = Vector2.SignedAngle(new Vector2(1, 0), worldGoal - comWorld) / 360.0f;
        if (ang < 0) ang += 1;
        float curAng = transform.rotation.eulerAngles.z / 360.0f - front;
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

        ApplyTorque(turn - cancelMoment);

        for (int i = 0; i < wheelForces.Length; i++)
        {
            // TODO: Remove debug
            Vector2 worldPos = transform.TransformPoint(wheelPositions[i]);
            Vector2 worldForce = transform.TransformDirection(wheelForces[i]);
            Debug.DrawLine(worldPos, worldPos + worldForce * 0.01f);

            ApplyForce(wheelForces[i], wheelPositions[i]);
        }
    }
}
