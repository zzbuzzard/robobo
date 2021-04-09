using System.Collections.Generic;
using UnityEngine;
using Mirror;

using XY = UnityEngine.Vector2Int;


[RequireComponent(typeof(Rigidbody2D))]
public class RobotScript : NetworkBehaviour
{
    // Wheel positions in integer, relative, local coords
    public Rigidbody2D mrig { get; private set; }

    // Map WheelType.HOVER -> List of positions, etc.
    public IDictionary<WheelType, List<XY>> wheelMap;
    private IDictionary<WheelType, int> initialWheelCounts;
    public WheelType currentWheelType { get; private set; }

    HoverMovementController hoverMovementController;
    TrackMovementController trackMovementController;

    public List<GameObject> children { get; private set; }
    private BlockGraph blockGraph;
    private IDictionary<XY, Block> blockDict;
    //private XY centerXY;
    private GameObject centerObj;

    public bool manual = false;


    ////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    /////////////////////////////////       Utilities      /////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    
    // Num wheels of given type
    public int NumWheels(WheelType wheel)
    {
        return wheelMap[wheel].Count;
    }

    // Num blocks of given type
    public int NumTypes(BlockType block)
    {
        int n = 0;
        foreach (Block b in blockDict.Values)
        {
            if (b.Type == block) n += 1;
        }
        return n;
    }

    // Load in block gameobjects based on this Robot object
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

            GameObject obj = Instantiate(prefab, new Vector2(pos.x * 1.5f, pos.y * 1.5f) + (Vector2)transform.position, angle);
            NetworkServer.Spawn(obj, connectionToClient);

            Block block = obj.GetComponent<Block>();
            block.x = pos.x;
            block.y = pos.y;
            block.SetParent(null, gameObject);
        }

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
        initialWheelCounts = new Dictionary<WheelType, int>();
        foreach (WheelType wheelType in BlockInfo.wheelTypes)
        {
            initialWheelCounts[wheelType] = wheelMap[wheelType].Count;
        }

        BlocksChanged();
        InitialiseGraph(robot);
    }

    // Loads from gameobjects which are already spawned
    public void LoadRobotClient(Robot robot)
    {
        mrig = GetComponent<Rigidbody2D>();

        hoverMovementController = new HoverMovementController(this);
        trackMovementController = new TrackMovementController(this);
        
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
        initialWheelCounts = new Dictionary<WheelType, int>();
        foreach (WheelType wheelType in BlockInfo.wheelTypes)
        {
            initialWheelCounts[wheelType] = wheelMap[wheelType].Count;
        }

        BlocksChanged();
        InitialiseGraph(robot);
    }


    // Position of control block in worldspace
    public Vector2 GetControlPos()
    {
        if (centerObj == null) return transform.position;
        return centerObj.transform.position;
        //return transform.TransformPoint(1.5f * (Vector2)centerXY);
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



    ////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    /////////////////////////////////     User control     /////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////

    public void Use()
    {
        if (isServer)
            ClientUse();

        foreach (GameObject g in children)
        {
            IUsableBlock b = g.GetComponent<IUsableBlock>();
            if (b != null)
            {
                b.Use();
            }
        }
    }

    [ClientRpc]
    private void ClientUse()
    {
        if (isLocalPlayer) return;
        Use();
    }

    // Move with no turn applied
    public void Move(Vector2 moveDir)
    {
        Move(moveDir, Vector2.zero, false);
    }

    // Moves the robot in direction moveDirection, and faces towards lookDirection (world coords)
    public void Move(Vector2 moveDirection, Vector2 lookDirection, bool isLooking = true)
    {
        if (lookDirection == Vector2.zero) isLooking = false;

        if (moveDirection.magnitude > 1.0f) moveDirection = moveDirection.normalized;

        if (currentWheelType != WheelType.NONE)
        {
            switch (currentWheelType)
            {
                case WheelType.HOVER:
                    hoverMovementController.Move(moveDirection, lookDirection, isLooking);
                    break;
                case WheelType.WHEEL:
                    break;
                case WheelType.TRACK:
                    trackMovementController.Move(moveDirection, lookDirection, isLooking);
                    break;
            }
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    /////////////////////////////////  Internal functions  /////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////

    private void Start()
    {
        if (manual)
            ManualInitialisation();

        // Otherwise, we wait for a LoadRobot call.
    }

    // Initialise BlockGraph based on Robot
    private void InitialiseGraph(Robot robot)
    {
        blockGraph = new BlockGraph(robot);

        // TODO: Remove this, it'll slow things down v slightly
        if (!blockGraph.IsValidRobot())
        {
            Debug.LogWarning(gameObject.name + "is an invalid robot!!!");
        }
    }

    // Guess the Robot based on the present GameObjects - should be testing use only
    private void ManualInitialisation() {
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
        initialWheelCounts = new Dictionary<WheelType, int>();
        foreach (WheelType wheelType in BlockInfo.wheelTypes)
        {
            initialWheelCounts[wheelType] = wheelMap[wheelType].Count;
        }

        BlocksChanged();
        InitialiseGraph(robot);
    }

    // Must be called after any change, e.g. lost a wheel/block
    private void BlocksChanged()
    {
        blockDict = new Dictionary<XY, Block>();
        children = new List<GameObject>();
        centerObj = null;

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

                if (b.Type == BlockType.CONTROL)
                {
                    centerObj = g;
                }
            }
        }

        // We have no children - just kill the gameobject.
        if (children.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        // Find the highest priority wheel which we have more than 0 of
        currentWheelType = WheelType.NONE;
        for (int i=0; i<BlockInfo.wheelTypes.Length; i++)
        {
            WheelType t = BlockInfo.wheelTypes[i];
            if (wheelMap[t].Count > 0)
            {
                currentWheelType = t;
                break;
            }
        }

        hoverMovementController.UpdateWheels(GetBlocksFromXYs(wheelMap[WheelType.HOVER]));
        trackMovementController.UpdateWheels(GetBlocksFromXYs(wheelMap[WheelType.TRACK]));

        // Power supplied to one wheel is total power / num wheels
        float power = GetRobotPower();

        hoverMovementController.WheelPower = 3.0f; // power / initialWheelCounts[WheelType.HOVER];
        trackMovementController.WheelPower = 3.0f; // power / initialWheelCounts[WheelType.TRACK];
    }

    private float GetRobotPower()
    {
        return 10.0f;

        //float power = 0.0f;
        //foreach (XY xy in blockDict.Keys)
        //{
        //    BlockType type = blockDict[xy].Type;
        //    if (type == BlockType.METAL) power += 1.0f;
        //    if (type == BlockType.CONTROL) power += 2.0f;
        //}
        //return power;
    }

    // Return a list of Blocks at the positions given by the XYs
    private List<Block> GetBlocksFromXYs(List<XY> xys)
    {
        List<Block> ans = new List<Block>();
        foreach (XY xy in xys)
        {
            ans.Add(blockDict[xy]);
        }
        return ans;
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
