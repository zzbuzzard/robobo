using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

[RequireComponent(typeof(Rigidbody2D))]
public class RobotScript : MonoBehaviour
{
    // Wheel positions in integer, relative, local coords
    public Rigidbody2D mrig { get; private set; }
    private bool initialised = false;

    // Map WheelType.HOVER -> List of positions, etc.
    public IDictionary<WheelType, List<XY>> wheelMap;
    HoverMovementController hoverMovementController;

    public List<GameObject> children { get; private set; }
    private BlockGraph blockGraph;
    private IDictionary<XY, Block> blockDict;
    private XY centerXY;

    // Load in gameobjects based on this Robot object
    public void LoadRobot(Robot robot)
    {
        initialised = true;
        mrig = GetComponent<Rigidbody2D>();
        transform.DetachChildren();

        hoverMovementController = new HoverMovementController(this);

        // Load in each block
        foreach (XY pos in robot.blockTypes.Keys)
        {
            BlockType type = robot.blockTypes[pos];
            GameObject prefab = BlockInfo.blockInfos[(int)type].prefab;

            float zrot = robot.rotations[pos] * 90.0f;
            Quaternion angle = Quaternion.Euler(0, 0, zrot);

            GameObject obj = Instantiate(prefab, new Vector2(pos.x * 1.5f, pos.y * 1.5f), angle, transform);
            Block block = obj.GetComponent<Block>();
            block.x = pos.x;
            block.y = pos.y;
        }

        centerXY = robot.center;

        // Put wheels into categories and give to controllers
        wheelMap = new Dictionary<WheelType, List<XY>>();
        foreach (WheelType wheelType in BlockInfo.wheelTypes) {
            wheelMap[wheelType] = new List<XY>();
        }
        foreach (XY xy in robot.wheels) {
            BlockType type = robot.blockTypes[xy];
            WheelType wheelType = BlockInfo.blockInfos[(int)type].wheelType;
            wheelMap[wheelType].Add(xy);
        }

        BlocksChanged();
        InitialiseGraph(robot);
    }

    public void Use()
    {
        foreach (GameObject g in children)
        {
            UsableWeaponBlock b = g.GetComponent<UsableWeaponBlock>();
            if (b != null)
            {
                b.Use();
            }
        }
    }

    // Ah, isn't that beautiful
    void InitialiseGraph(Robot robot) {
        blockGraph = new BlockGraph(robot);

        // TODO: Remove this, it'll slow things down v slightly
        if (!blockGraph.IsValidRobot()) {
            Debug.LogWarning(gameObject.name + "is an invalid robot!!!");
        }
    }

    // TODO: Unscuff or just remove tbh
    void InitialiseGraphScuffed() {
        initialised = true;
        centerXY = new XY(0, 0);

        List<Block> blocks = new List<Block>();
        int index = 0;
        int control = -1;

        foreach (GameObject g in children)
        {
            Block b = g.GetComponent<Block>();
            if (b != null)
            {
                if (b.Type == BlockType.CONTROL) control = index;
                blocks.Add(b);
                index++;
            }
        }

        if (control == -1)
        {
            Debug.LogWarning("No control block in " + gameObject.name);
            return;
        }
        
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

        // Delete from wheels if it's a wheel
        if (a.Wheel != WheelType.NONE)
            wheelMap[a.Wheel].Remove(pos);

        List<XY> deaths = blockGraph.RemoveAllUnreachable();
        foreach (XY xy in deaths)
        {
            Block b = blockDict[xy];
            b.Detach();

            // Delete from wheels if wheel
            if (b.Wheel != WheelType.NONE)
                wheelMap[a.Wheel].Remove(new XY(b.x, b.y));
        }
        BlocksChanged();
    }

    // World space
    public Vector2 GetControlPos()
    {
        return transform.TransformPoint(1.5f * (Vector2)centerXY);
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

        hoverMovementController.UpdateWheels(wheelMap[WheelType.HOVER]);
    }


    // World move + World look
    public void Move(Vector2 moveDirection, Vector2 lookDirection)
    {
        // TODO: Pick the right one depending on a variable
        hoverMovementController.Move(moveDirection, lookDirection);
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
