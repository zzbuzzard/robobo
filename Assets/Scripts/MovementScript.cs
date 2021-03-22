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
    //private Vector2[] wheelForces;
    private Vector2[] turnOneUnit;

    private List<GameObject> children;

    Rigidbody2D mrig;

    //private float MASS;
    private float SMOA;

    public float dampConst = 0.5f; // 2 is perfect critical damping, lower is a faster but wobblier turn
    public float moveForce = 500;
    public float turnForce = 10000.0f; // eek this is a bit high

    BlockGraph blockGraph;
    public void InitialiseGraph()
    {
        List<Block> blocks = new List<Block>();
        int index = 0;
        int control = -1;

        foreach (GameObject g in children)
        {
            Block b = g.GetComponent<Block>();
            if (b != null)
            {
                if (b.IsControl()) control = index;
                blocks.Add(b);
                index++;
            }
        }

        if (control == -1)
        {
            Debug.LogError("No control block in " + gameObject.name);
            return;
        }

        blockGraph = new BlockGraph(blocks, control);
    }

    public void RemoveBlock(Block a)
    {
        List<Block> deaths = blockGraph.KillComponent(a);
        foreach (Block b in deaths)
        {
            b.Die();
        }
        BlocksChanged();
    }

    public void ApplyTorque(float f)
    {
        for (int i = 0; i < wheelPositions.Count; i++)
        {
            ApplyForce(turnOneUnit[i] * f, wheelPositions[i]);
            //wheelForces[i] += turnOneUnit[i] * f;
        }
    }

    // returns the moment produced
    // currently sets the force on every wheel to f
    public float ApplyMovement(Vector2 f)
    {
        f *= moveForce;

        float moment = 0;
        for (int i = 0; i < wheelPositions.Count; i++)
        {
            //wheelForces[i] = f;
            ApplyForce(f, wheelPositions[i]);
            Vector2 comToPos = wheelPositions[i] - mrig.centerOfMass;
            moment += Vector3.Cross(comToPos, f).z;
        }
        return moment;
    }

    public float CalculateTorque(float angle)
    {
        float c = dampConst * Mathf.Sqrt(SMOA * turnForce);
        float dampingMoment = c * mrig.angularVelocity * Mathf.Deg2Rad;
        float springMoment = angle * turnForce;

        return Mathf.Clamp(springMoment - dampingMoment, -turnForce, turnForce);
    }


    // PRIVATE FUNCTIONS:

    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();
        BlocksChanged();
        InitialiseGraph();
    }


    // e.g. lost a wheel/block
    // TODO: Lose wheels when containing block is lost, or something?    
    private void BlocksChanged()
    {
        // Load children
        children = new List<GameObject>();
        int c = transform.childCount;
        for (int i = 0; i < c; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }

        // We have no children - just kill the gameobject.
        if (children.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        //wheelForces = new Vector2[wheelPositions.Count];
        turnOneUnit = new Vector2[wheelPositions.Count];

        LoadStats();
        LoadTurnOneUnit();
    }

    // TODO: I scuffed arnavs calculation (in my defence, you don't seem to be able to do collider.area)
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

    private void ApplyForce(Vector2 localForce, Vector2 localPos)
    {
        Vector2 worldPos = transform.TransformPoint(localPos);
        Vector2 worldForce = transform.TransformDirection(localForce);
        mrig.AddForceAtPosition(worldForce, worldPos);
    }

    void FixedUpdate()
    {
        // I miss this debugging thing ;(
        // could add it back but wheelForces is gone
        //for (int i = 0; i < wheelPositions.Count; i++)
        //{
        //    Vector2 worldPos = transform.TransformPoint(wheelPositions[i]);
        //    Vector2 worldForce = transform.TransformDirection(wheelForces[i]);
        //    Debug.DrawLine(worldPos, worldPos + worldForce * 0.01f);
        //}
    }
}
