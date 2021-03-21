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

public class ControlBlock : MonoBehaviour
{
    public List<Rigidbody2D> wheels;
    private List<Rigidbody2D> children;
    //private List<Vector2> childOffsets;
    private List<int> wheelToChild;
    private List<Vector2> wheelForces;

    private Vector2[] turnOneUnit;
    private Vector2 COM; // local coords
    private float MASS;
    private float SMOA;

    // 100, 5000
    private float dampConst = 0.45f; // 2 is perfect critical damping, lower is faster turn but wobblier
    private float moveForce = 250;
    private float turnForce = 6000.0f;

    // Front angle
    private float front = 0.75f;

    void Start()
    {
        // Load children rigidbodies from transform.children
        children = new List<Rigidbody2D>();
        //childOffsets = new List<Vector2>();
        wheelToChild = new List<int>();

        int c = transform.childCount;
        for (int i = 0; i < c; i++)
        {
            Rigidbody2D rig = transform.GetChild(i).GetComponent<Rigidbody2D>();
            if (rig != null)
            {
                children.Add(rig);
                //childOffsets.Add(rig.transform.localPosition);
            }
        }

        // Initialises COM and turn forces
        BlocksChanged();
    }

    // e.g. lost a wheel/block
    // TODO: remove from children/wheel list
    private void BlocksChanged()
    {
        wheelForces = new List<Vector2>();
        turnOneUnit = new Vector2[wheels.Count];

        wheelToChild.Clear();
        // Update wheelToChild
        for (int i = 0; i < wheels.Count; i++)
        {
            wheelForces.Add(Vector2.zero);

            int x = -1;
            for (int j = 0; j < children.Count; j++)
            {
                if (wheels[i] == children[j])
                {
                    x = j;
                    break;
                }
            }
            if (x == -1)
            {
                Debug.LogWarning("COULDNT FIND A WHEEL IN THE CHILD LIST");
            }
            wheelToChild.Add(x);
        }

        LoadStats();
        LoadTurnOneUnit();
    }

    private void LoadStats()
    {
        Vector2 tot = Vector2.zero;
        float totM = 0;
        float smoa = 0;
        for (int i = 0; i < children.Count; i++)
        {
            Rigidbody2D r = children[i];
            //Vector2 off = childOffsets[i];

            tot += r.mass * r.worldCenterOfMass; //+ off - (Vector2)r.transform.localPosition);
            totM += r.mass;
        }

        COM = tot / totM;
        for (int i = 0; i < children.Count; i++)
        {
            Rigidbody2D r = children[i];
            smoa += r.mass * Mathf.Pow((r.worldCenterOfMass - COM).magnitude, 2.0f);
        }
        MASS = totM;
        SMOA = smoa;
    }

    private void LoadTurnOneUnit()
    {
        int N = wheels.Count;
        Vector2 totForce = Vector2.zero;

        for (int i = 0; i < N - 1; i++)
        {
            Vector2 comToPos = (Vector2)wheels[i].transform.position - COM;
            Vector2 rotated = Vector2.Perpendicular(comToPos);
            turnOneUnit[i] = rotated;
            totForce += turnOneUnit[i];
        }

        float moment = 0;
        turnOneUnit[N - 1] = -totForce;
        for (int i = 0; i < N; i++)
        {
            Vector2 comToPos = (Vector2)wheels[i].transform.position - COM;
            moment += Vector3.Cross(comToPos, turnOneUnit[i]).z;
        }

        // MOMENT CANCELLED OUT: THEY CAN'T TURN
        if (moment == 0)
        {
            Debug.LogWarning("CAN'T TURN");
            return;
        }

        if (Mathf.Abs(moment) < 0.5f) moment = 0.5f * moment / Mathf.Abs(moment);
        for (int i = 0; i < N; i++)
        {
            turnOneUnit[i] *= 1.0f / moment;
        }
    }

    //private void MoveUpdate(float force)
    //{
    //    Vector2 movement = Vector2.zero;

    //    // left
    //    if (Input.GetKey(KeyCode.A))
    //    {
    //        movement.x -= 1;
    //    }

    //    // right
    //    if (Input.GetKey(KeyCode.D))
    //    {
    //        movement.x += 1;
    //    }

    //    // up
    //    if (Input.GetKey(KeyCode.W))
    //    {
    //        movement.y += 1;
    //    }

    //    // down
    //    if (Input.GetKey(KeyCode.S))
    //    {
    //        movement.y -= 1;
    //    }

    //    foreach (Rigidbody2D r in wheels)
    //        r.AddForce(movement * force);
    //}

    //private void LookUpdate(float turnPower)
    //{
    //    foreach (Rigidbody2D r in wheels)
    //    {
    //        Vector2 pos = r.worldCenterOfMass;
    //        Vector2 worldGoal = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    //        // Goal angle, -0.5 ... 0.5
    //        float ang = Mathf.Atan2(worldGoal.y - pos.y, worldGoal.x - pos.x) / (2 * Mathf.PI);
    //        // Current angle, 0 ... 1
    //        float curAng = r.gameObject.transform.rotation.eulerAngles.z / 360.0f - front;

    //        //float currentVel = rig.angularVelocity;
    //        //Mathf.SmoothDampAngle(curAng * 360.0f, ang * 360.0f, ref currentVel, 0.001f, Mathf.Infinity, Time.fixedDeltaTime);
    //        //rig.angularVelocity = currentVel;

    //        // Take the shortest path between them (acw or cw)
    //        float c1 = ang - curAng, c2 = ang - curAng + 1, c;
    //        if (Mathf.Abs(c1) < Mathf.Abs(c2)) c = c1;
    //        else c = c2;

    //        float vel = r.angularVelocity;
    //        if (vel * c > 0) // same sign
    //        {
    //            c /= Mathf.Max(1.0f, 30 * Mathf.Abs(vel) / 360.0f);
    //        }

    //        r.AddTorque(c * turnPower);
    //    }
    //}

    private void ApplyTorque(float f)
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            wheelForces[i] += turnOneUnit[i] * f;
        }
    }

    // returns torque produced
    // sets the force on every wheel to f
    private float ApplyMovement(Vector2 f)
    {
        float moment = 0;
        for (int i = 0; i < wheels.Count; i++)
        {
            wheelForces[i] = f;
            Vector2 comToPos = (Vector2)wheels[i].transform.position - COM;
            moment += Vector3.Cross(comToPos, f).z;
        }
        return moment;
    }

    private float CalculateTorque(float angle)
    {
        float c = dampConst * Mathf.Sqrt(SMOA * turnForce);
        float dampingMoment = c * children[0].angularVelocity * Mathf.Deg2Rad;
        float springMoment = angle * turnForce;

        return Mathf.Clamp(springMoment - dampingMoment, -turnForce, turnForce);
    }

    void FixedUpdate()
    {
        LoadStats();
        LoadTurnOneUnit();

        Vector2 movement = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            movement.x -= 1;
        if (Input.GetKey(KeyCode.D))
            movement.x += 1;
        if (Input.GetKey(KeyCode.W))
            movement.y += 1;
        if (Input.GetKey(KeyCode.S))
            movement.y -= 1;

        float cancelMoment = ApplyMovement(movement * moveForce);

        Vector2 worldGoal = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 comWorld = COM;

        float ang = Vector2.SignedAngle(new Vector2(1, 0), worldGoal - comWorld) / 360.0f;
        if (ang < 0) ang += 1;
        float curAng = wheels[0].gameObject.transform.rotation.eulerAngles.z / 360.0f - front;
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

        for (int i = 0; i < wheelForces.Count; i++)
        {
            Debug.DrawLine(wheels[i].transform.position, wheels[i].transform.position + (Vector3)wheelForces[i] / 50.0f);
            wheels[i].AddForce(wheelForces[i]);
        }
    }
}
