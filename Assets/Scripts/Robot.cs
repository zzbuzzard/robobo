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

    // Takes a map of (position) to a tuple (block type, rotation)
    //  (rotation is an int, 0..3)
    // Then the XY of the center, followed by the wheel positions
    public Robot(IDictionary<XY, BlockType> blockTypes, IDictionary<XY, int> rotations)
    {
        this.blockTypes = blockTypes;
        this.rotations = rotations;

        wheels = new List<XY>();

        int c = 0;
        foreach (XY xy in blockTypes.Keys)
        {
            BlockType type = blockTypes[xy];
            
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
                int index = Random.Range(0, spaces.Count);
                XY xy = spaces[index];
                spaces.RemoveAt(index);

                BlockType chosen = BlockType.METAL;
                if (Random.Range(0.0f, 1.0f) < wheelProb || !hasWheel)
                {
                    hasWheel = true;
                    chosen = BlockType.HOVER;
                }

                if (blockGraph.CanPlace(xy, 0, chosen))
                {
                    blockGraph.AddBlock(xy, 0, chosen);
                    foreach (XY xy2 in BlockInfo.blockInfos[(int)chosen].shape.GetJoins(xy.x, xy.y, 0)) {
                        if (!blockGraph.IsOccupied(xy2))
                            spaces.Add(xy2);
                    }
                    break;
                }
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

                bool placed = false;

                for (int r = 0; r < 4; r++)
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

    public static void SaveRobotToFile(Robot robot, string name)
    {
        string path = Application.persistentDataPath + "/" + name + ".robot";
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

    public static Robot LoadRobotFromFile(string name)
    {
        string path = Application.persistentDataPath + "/" + name + ".robot";
        FileStream file;

        if (!File.Exists(path))
        {
            Debug.LogError("File didn't exist! Robot name " + name);
        }

        file = File.OpenRead(path);

        // FORMAT: number, [x, y, BlockType, name]
        BinaryReader reader = new BinaryReader(file);
        Int32 count = reader.ReadInt32();

        IDictionary<XY, BlockType> blockTypes = new Dictionary<XY, BlockType>();
        IDictionary<XY, int> rotations = new Dictionary<XY, int>();

        for (int i=0; i<count; i++)
        {
            int x     = reader.ReadInt32();
            int y     = reader.ReadInt32();
            int block = reader.ReadInt16();
            int rot   = reader.ReadInt16();

            XY pos = new XY(x, y);
            BlockType blockType = (BlockType)Enum.ToObject(typeof(BlockType), block);

            blockTypes[pos] = blockType;
            rotations[pos] = rot;
        }
        file.Close();

        return new Robot(blockTypes, rotations);
    }
}
