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

[RequireComponent(typeof(Rigidbody2D))]
public class MovementScript : MonoBehaviour
{
    // Positions
    public List<XY> wheels;

    private Vector2[] turnOneUnit;

    private List<GameObject> children;

    Rigidbody2D mrig;

    BlockGraph blockGraph;
    IDictionary<XY, Block> blockDict;
    private bool initialised = false;

    //private float MASS;
    private float SMOA;

    public float dampConst = 0.5f; // 2 is perfect critical damping, lower is a faster but wobblier turn
    public float moveForce = 500;
    public float turnForce = 10000.0f; // eek this is a bit high

    // Load in gameobjects based on this Robot object
    public void LoadRobot(Robot robot)
    {
        initialised = true;
        mrig = GetComponent<Rigidbody2D>();
        transform.DetachChildren();

        // Load in each block
        foreach (XY pos in robot.blockTypes.Keys)
        {
            BlockType type = robot.blockTypes[pos];
            GameObject prefab = BlockInfo.blockTypePrefabs[(int)type];

            float zrot = robot.rotations[pos] * 90.0f;
            Quaternion angle = Quaternion.Euler(0, 0, zrot);

            GameObject obj = Instantiate(prefab, new Vector2(pos.x * 1.5f, pos.y * 1.5f), angle, transform);
            Block block = obj.GetComponent<Block>();
            block.x = pos.x;
            block.y = pos.y;
        }

        wheels = robot.wheels;

        BlocksChanged();
        InitialiseGraph(robot);
    }

    // Ah, isn't that beautiful
    void InitialiseGraph(Robot robot) {
        Debug.Log("Initialise graph non scuffed");
        blockGraph = new BlockGraph(robot);

        // TODO: Remove this, it'll slow things down v slightly
        if (!blockGraph.IsValidRobot()) {
            Debug.LogWarning(gameObject.name + "is an invalid robot!!!");
        }
    }

    // Guesses the graph from the components
    // Ok not guess it's not that bad it's just a bit scuffed
    void InitialiseGraphScuffed() {
        initialised = true;

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
            Debug.LogWarning("No control block in " + gameObject.name);
            return;
        }

        Debug.Log("Scuffed initialisation");
        blockGraph = new BlockGraph();
        foreach (Block b in blocks) {
            // no dude thats too scuffed
            blockGraph.AddBlock(new XY(b.x, b.y), 0, BlockType.METAL);
        }
    }

    // Removes a block, and detaches all those who are no longer connected
    public void RemoveBlock(Block a)
    {
        XY pos = new XY(a.x, a.y);
        blockGraph.RemoveAt(pos);

        // Delete from wheels if wheel
        if (a.GetWheelType() != Block.WheelType.NONE)
            wheels.Remove(pos);

        List<XY> deaths = blockGraph.RemoveAllUnreachable();
        foreach (XY xy in deaths)
        {
            Block b = blockDict[xy];
            b.Detach();

            // Delete from wheels if wheel
            if (b.GetWheelType() != Block.WheelType.NONE)
                wheels.Remove(new XY(b.x, b.y));
        }
        BlocksChanged();
    }

    public void ApplyTorque(float f)
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            // TODO: Should this be through the COM? Though, it doesnt have a COM as it has no rigidbody
            Vector2 wheelPos = blockDict[wheels[i]].transform.localPosition;

            ApplyForce(turnOneUnit[i] * f, wheelPos);
        }
    }

    // returns the moment produced
    // currently sets the force on every wheel to f
    public float ApplyMovement(Vector2 f)
    {
        f *= moveForce;

        float moment = 0;
        for (int i = 0; i < wheels.Count; i++)
        {
            // TODO: Should this be through the COM? Though, it doesnt have a COM as it has no rigidbody
            Vector2 wheelPos = blockDict[wheels[i]].transform.localPosition;

            ApplyForce(f, wheelPos);
            Vector2 comToPos = wheelPos - mrig.centerOfMass;
            moment += Vector3.Cross(comToPos, f).z;
        }
        return moment;
    }

    public float CalculateTorque(float angle)
    {
        float maxTurn = turnForce * wheels.Count;

        float c = dampConst * Mathf.Sqrt(SMOA * maxTurn);
        float dampingMoment = c * mrig.angularVelocity * Mathf.Deg2Rad;
        float springMoment = angle * maxTurn;

        return Mathf.Clamp(springMoment - dampingMoment, -maxTurn, maxTurn);
    }


    // PRIVATE FUNCTIONS:

    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();

        // Assumes that we are being loaded directly
        if (!initialised && transform.childCount > 0)
        {
            BlocksChanged();
            InitialiseGraphScuffed();
        }
        // Otherwise, we wait for a LoadRobot call.
    }

    // e.g. lost a wheel/block
    // TODO: Lose wheels when containing block is lost, or something?
    private void BlocksChanged()
    {
        blockDict = new Dictionary<XY, Block>();
        children = new List<GameObject>();

        // Load children and blockDict
        int c = transform.childCount;
        for (int i = 0; i < c; i++)
        {
            GameObject g = transform.GetChild(i).gameObject;
            Block b = g.GetComponent<Block>();
            if (b != null && !b.IsDead())
            {
                children.Add(g);
                blockDict[new XY(b.x, b.y)] = b;
            }
        }

        // We have no children - just kill the gameobject.
        if (children.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        //wheelForces = new Vector2[wheelPositions.Count];
        turnOneUnit = new Vector2[wheels.Count];

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
        int N = wheels.Count;
        if (N == 0) return;
        Vector2 totForce = Vector2.zero;
        Vector2 com = mrig.centerOfMass;

        for (int i = 0; i < N - 1; i++)
        {
            Vector2 wheelPos = blockDict[wheels[i]].transform.localPosition;
            Vector2 comToPos = wheelPos - com;
            Vector2 rotated = Vector2.Perpendicular(comToPos);
            turnOneUnit[i] = rotated;
            totForce += turnOneUnit[i];
        }

        float moment = 0;
        turnOneUnit[N - 1] = -totForce;
        for (int i = 0; i < N; i++)
        {
            Vector2 wheelPos = blockDict[wheels[i]].transform.localPosition;
            Vector2 comToPos = wheelPos - com;
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
        Debug.DrawLine(worldPos, worldPos + worldForce * 0.1f, Color.green);
    }

    //void FixedUpdate()
    //{
        // I miss this debugging thing ;(
        // could add it back but wheelForces is gone
        //for (int i = 0; i < wheelPositions.Count; i++)
        //{
        //    Vector2 worldPos = transform.TransformPoint(wheelPositions[i]);
        //    Vector2 worldForce = transform.TransformDirection(wheelForces[i]);
        //    Debug.DrawLine(worldPos, worldPos + worldForce * 0.01f);
        //}
    //}
}
