using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

[RequireComponent(typeof(Rigidbody2D))]
public class RobotScript : MonoBehaviour
{
    // Wheel positions in integer, relative, local coords
    public Rigidbody2D mrig { get; private set; }

    // Map WheelType.HOVER -> List of positions, etc.
    public IDictionary<WheelType, List<XY>> wheelMap;

    HoverMovementController hoverMovementController;
    TrackMovementController trackMovementController;

    public List<GameObject> children { get; private set; }
    private BlockGraph blockGraph;
    private IDictionary<XY, Block> blockDict;
    private XY centerXY;

    public bool manual = false;

    // Load in gameobjects based on this Robot object
    public void LoadRobot(Robot robot)
    {
        mrig = GetComponent<Rigidbody2D>();
        transform.DetachChildren();

        hoverMovementController = new HoverMovementController(this);
        trackMovementController = new TrackMovementController(this);

        // Load in each block
        foreach (XY pos in robot.blockTypes.Keys)
        {
            BlockType type = robot.blockTypes[pos];
            GameObject prefab = BlockInfo.blockInfos[(int)type].prefab;

            float zrot = robot.rotations[pos] * 90.0f;
            Quaternion angle = Quaternion.Euler(0, 0, zrot);

            GameObject obj = Instantiate(prefab, new Vector2(pos.x * 1.5f, pos.y * 1.5f) + (Vector2)transform.position, angle, transform);
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

    // Should only be used in testing - slightly dodgy
    void ManualInitialisation() {
        IDictionary<XY, BlockType> blockTypes = new Dictionary<XY, BlockType>();
        IDictionary<XY, int> rotations = new Dictionary<XY, int>();

        for (int i=0; i<transform.childCount; i++)
        {
            GameObject g = transform.GetChild(i).gameObject;
            Block b = g.GetComponent<Block>();
            if (b == null || b.IsDead()) continue;

            Vector2 p = g.transform.localPosition;
            int x = Mathf.RoundToInt(p.x / 1.5f);
            int y = Mathf.RoundToInt(p.y / 1.5f);
            XY xy = new XY(x, y);

            int rot = Mathf.RoundToInt(g.transform.localRotation.eulerAngles.z / 90.0f);

            blockTypes[xy] = b.Type;
            rotations[xy] = rot;

            b.x = x;
            b.y = y;
        }

        Robot robot = new Robot(blockTypes, rotations);

        // Copy paste from robot
        mrig = GetComponent<Rigidbody2D>();
        hoverMovementController = new HoverMovementController(this);
        trackMovementController = new TrackMovementController(this);

        centerXY = robot.center;

        // Put wheels into categories and give to controllers
        wheelMap = new Dictionary<WheelType, List<XY>>();
        foreach (WheelType wheelType in BlockInfo.wheelTypes)
        {
            wheelMap[wheelType] = new List<XY>();
        }
        foreach (XY xy in robot.wheels)
        {
            BlockType type = robot.blockTypes[xy];
            WheelType wheelType = BlockInfo.blockInfos[(int)type].wheelType;
            wheelMap[wheelType].Add(xy);
        }

        BlocksChanged();
        InitialiseGraph(robot);
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
                wheelMap[b.Wheel].Remove(new XY(b.x, b.y));
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
        if (manual)
            ManualInitialisation();

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

        hoverMovementController.UpdateWheels(wheelMap[WheelType.HOVER], ScuffedRotFromRotation(wheelMap[WheelType.HOVER]));
        trackMovementController.UpdateWheels(wheelMap[WheelType.TRACK], ScuffedRotFromRotation(wheelMap[WheelType.TRACK]));
    }

    // Okk, not even that scuffed
    private List<int> ScuffedRotFromRotation(List<XY> blocks)
    {
        List<int> ans = new List<int>();
        foreach (XY xy in blocks)
        {
            Block b = blockDict[xy];
            int r = Mathf.RoundToInt(b.transform.localRotation.eulerAngles.z / 90.0f);
            ans.Add(r);
        }
        return ans;
    }

    // World move + World look
    public void Move(Vector2 moveDirection, Vector2 lookDirection)
    {
        // TODO: Pick the right one depending on a variable
        hoverMovementController.Move(moveDirection, lookDirection);
        trackMovementController.Move(moveDirection, lookDirection);
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
