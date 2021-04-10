using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;
using XY = UnityEngine.Vector2Int;

// A class to store a robot (its blocks, stats, etc...)
public class Robot
{
    public IDictionary<XY, BlockType> blockTypes;
    public IDictionary<XY, int> rotations;

    public XY center;
    public List<XY> wheels;

    public readonly int cost;

    // Takes a map of (position) to a tuple (block type, rotation)
    //  (rotation is an int, 0..3)
    // Then the XY of the center, followed by the wheel positions
    public Robot(IDictionary<XY, BlockType> blockTypes, IDictionary<XY, int> rotations)
    {
        this.blockTypes = blockTypes;
        this.rotations = rotations;

        wheels = new List<XY>();
        cost = 0;

        int c = 0;
        foreach (XY xy in blockTypes.Keys)
        {
            BlockType type = blockTypes[xy];

            cost += BlockInfo.blockInfos[(int)type].cost;

            // Check if control
            if (type == BlockType.CONTROL)
            {
                c++;
                center = xy;
            }

            // Check if wheel
            if (BlockInfo.wheelBlocks.Contains(type))
            {
                wheels.Add(xy);
            }
        }

        if (c != 1)
        {
            Debug.LogWarning("Constructing a robot with the incorrect number of control blocks: " + c);
        }
    }

    public static Robot GenerateRandomRobot(int blockNum, int weaponNum, float wheelProb = 0.2f)
    {
        BlockGraph blockGraph = new BlockGraph();
        blockGraph.AddBlock(new XY(0, 0), 0, BlockType.CONTROL);

        IList<XY> spaces = new List<XY>();
        foreach (XY xy in BlockInfo.blockInfos[(int)BlockType.CONTROL].shape.GetJoins(0, 0, 0))
            spaces.Add(xy);

        bool hasWheel = false;

        // Place body
        for (int _=0; _<blockNum-1; _++)
        {
            while (spaces.Count > 0)
            {
                int index = Random.Range(0, spaces.Count - 1);
                XY xy = spaces[index];
                spaces.RemoveAt(index);
                BlockType chosen;
                if (!hasWheel || Random.Range(0, 1.0f) < wheelProb)
                {
                    chosen = BlockInfo.wheelBlocks[Random.Range(0, BlockInfo.wheelBlocks.Length)];
                }
                else
                { 
                    chosen = BlockType.METAL;
                }
                int maxr = BlockInfo.blockInfos[(int)chosen].maxRot;

                bool placed = false;

                for (int r = 0; r <= maxr; r++)
                {
                    if (blockGraph.CanPlace(xy, r, chosen))
                    {
                        blockGraph.AddBlock(xy, r, chosen);
                        placed = true;
                        foreach (XY xy2 in BlockInfo.blockInfos[(int)chosen].shape.GetJoins(xy.x, xy.y, r))
                        {
                            if (!blockGraph.IsOccupied(xy2))
                                spaces.Add(xy2);
                        }
                        break;
                    }
                }

                if (placed) break;

            }
        }

        // Place weapons
        for (int _ = 0; _ < weaponNum; _++)
        {
            while (spaces.Count > 0)
            {
                int index = Random.Range(0, spaces.Count - 1);
                XY xy = spaces[index];
                spaces.RemoveAt(index);
                BlockType chosen = BlockInfo.weapons[Random.Range(0, BlockInfo.weapons.Length)];
                int maxr = BlockInfo.blockInfos[(int)chosen].maxRot;

                bool placed = false;

                for (int r = 0; r <= maxr; r++)
                {
                    if (blockGraph.CanPlace(xy, r, chosen))
                    {
                        blockGraph.AddBlock(xy, r, chosen);
                        placed = true;
                        foreach (XY xy2 in BlockInfo.blockInfos[(int)chosen].shape.GetJoins(xy.x, xy.y, r))
                        {
                            if (!blockGraph.IsOccupied(xy2))
                                spaces.Add(xy2);
                        }
                        break;
                    }
                }

                if (placed) break;
            }
        }

        IDictionary<XY, BlockType> map = new Dictionary<XY, BlockType>();
        IDictionary<XY, int> map2 = new Dictionary<XY, int>();

        foreach (XY xy in blockGraph.GetBlockPositions()) {
            BlockGraph.BlockData data = blockGraph.GetBlockData(xy);
            map[xy] = data.type;
            map2[xy] = data.rot;
        }

        return new Robot(map, map2);
    }

    public static Robot RandomFileRobot()
    {
        const int maxnum = 16;

        int pick = Random.Range(0, maxnum + 1);
        string path = Path.Combine("Robots", "robot" + pick);
        
        TextAsset file = Resources.Load<TextAsset>(path);

        byte[] byteArray = file.bytes;
        MemoryStream stream = new MemoryStream(byteArray);
        BinaryReader reader = new BinaryReader(stream);

        Robot r = LoadRobotFromReader(reader);
        reader.Close();
        stream.Close();

        return r;
    }

    private static string GetRobotDir()
    {
        string path = Application.persistentDataPath + "/Robots";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    public static void SaveRobotToFile(Robot robot, string name)
    {
        string path = GetRobotDir() + "/" + name + ".robot";

        FileStream file;

        if (File.Exists(path)) file = File.OpenWrite(path);
        else file = File.Create(path);

        // Save block types, rotations
        // FORMAT: number, [x, y, BlockType, name]
        BinaryWriter writer = new BinaryWriter(file);


        Int32 count = (Int32)robot.blockTypes.Count;
        writer.Write(count);
        foreach (XY xy in robot.blockTypes.Keys)
        {
            Int32 x = (Int32)xy.x;
            Int32 y = (Int32)xy.y;
            Int16 block = (Int16)robot.blockTypes[xy];
            Int16 rot = (Int16)robot.rotations[xy];

            writer.Write(x);
            writer.Write(y);
            writer.Write(block);
            writer.Write(rot);
        }

        writer.Close();
        file.Close();
    }

    private static Robot LoadRobotFromReader(BinaryReader reader)
    {
        // FORMAT: number, [x, y, BlockType, name]
        Int32 count = reader.ReadInt32();

        IDictionary<XY, BlockType> blockTypes = new Dictionary<XY, BlockType>();
        IDictionary<XY, int> rotations = new Dictionary<XY, int>();

        for (int i = 0; i < count; i++)
        {
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            int block = reader.ReadInt16();
            int rot = reader.ReadInt16();

            XY pos = new XY(x, y);
            BlockType blockType = (BlockType)Enum.ToObject(typeof(BlockType), block);

            blockTypes[pos] = blockType;
            rotations[pos] = rot;
        }

        return new Robot(blockTypes, rotations);
    }

    public static Robot LoadRobotFromName(string name)
    {
        string path = GetRobotDir() + "/" + name + ".robot";
        FileStream file;

        // Expected behaviour- this happens when making a new robot
        if (!File.Exists(path))
        {
            Debug.Log("File didn't exist for robot " + name);
            return null;
        }

        file = File.OpenRead(path);

        // FORMAT: number, [x, y, BlockType, name]
        BinaryReader reader = new BinaryReader(file);
        Robot r = LoadRobotFromReader(reader);
        reader.Close();
        file.Close();
        return r;
    }

    public static IDictionary<string, Robot> LoadAllRobots()
    {
        IDictionary<string, Robot> dict = new Dictionary<string, Robot>();
        string path = GetRobotDir();
        foreach (string robopath in Directory.GetFiles(path))
        {
            if (!robopath.EndsWith(".robot")) continue;

            string p = robopath.Substring(path.Length + 1); // +1 for the /
            p = p.Substring(0, p.Length - 6); // .robot = 6 chars

            dict[p] = LoadRobotFromName(p);
        }

        return dict;
    }

    public struct SerializedRobot
    {
        public Int16[] xs, ys, blockTypes, rots;
    }

    // Takes a robot, returns it in a form that Mirror is happy to transmit
    public static SerializedRobot SerializeRobot(Robot r)
    {
        SerializedRobot s;

        int N = r.blockTypes.Count;
        s.xs = new Int16[N];
        s.ys = new Int16[N];
        s.blockTypes = new Int16[N];
        s.rots = new Int16[N];

        int i = 0;
        foreach (XY xy in r.blockTypes.Keys)
        {
            Int16 x = (Int16)xy.x;
            Int16 y = (Int16)xy.y;
            Int16 block = (Int16)r.blockTypes[xy];
            Int16 rot = (Int16)r.rotations[xy];

            s.xs[i] = x;
            s.ys[i] = y;
            s.blockTypes[i] = block;
            s.rots[i] = rot;

            i++;
        }

        return s;
    }

    // Takes a serialized robot and returns a robot
    public static Robot DeserializeRobot(SerializedRobot r)
    {
        IDictionary<XY, BlockType> blockTypes = new Dictionary<XY, BlockType>();
        IDictionary<XY, int> rotations = new Dictionary<XY, int>();

        // TODO: Check for errors; may have been fucked with or lost during transmission
        for (int i=0; i<r.xs.Length; i++)
        {
            XY xy = new XY(r.xs[i], r.ys[i]);
            BlockType b = (BlockType)r.blockTypes[i];

            blockTypes[xy] = b;
            rotations[xy] = r.rots[i];
        }

        return new Robot(blockTypes, rotations);
    }
}