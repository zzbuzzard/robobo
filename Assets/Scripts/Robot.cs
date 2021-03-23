using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

// A class to store a robot (its blocks, stats, etc...)
public class Robot
{
    public IDictionary<XY, BlockType> blocks;
    public IDictionary<XY, int> rotation;
    public XY center;
    public List<Vector2> wheels;

    // Takes a map of (position) to a tuple (block type, rotation)
    //  (rotation is an int, 0..3)
    // Then the XY of the center, followed by the wheel positions
    public Robot(IDictionary<XY, System.Tuple<BlockType, int>> blocks, XY center, List<Vector2> wheels)
    {
        this.blocks = new Dictionary<XY, BlockType>();
        this.rotation = new Dictionary<XY, int>();
        foreach (XY xy in blocks.Keys)
        {
            this.blocks[xy]   = blocks[xy].Item1;
            this.rotation[xy] = blocks[xy].Item2;
        }

        this.center = center;
        this.wheels = wheels;
    }

    public static Robot GenerateRandomRobot(int blockNum, int weaponNum)
    {
        BlockGraph blockGraph = new BlockGraph();
        blockGraph.AddBlock(new XY(0, 0), 0, BlockType.CONTROL);

        IList<XY> spaces = new List<XY>();
        foreach (XY xy in BlockInfo.blockTypeShapes[(int)BlockType.CONTROL].GetJoins(0, 0, 0))
            spaces.Add(xy);

        // Place body
        for (int _=0; _<blockNum-1; _++)
        {
            while (spaces.Count > 0)
            {
                int index = Random.Range(0, spaces.Count);
                XY xy = spaces[index];
                spaces.RemoveAt(index);

                if (blockGraph.CanPlace(xy, 0, BlockType.METAL))
                {
                    blockGraph.AddBlock(xy, 0, BlockType.METAL);
                    foreach (XY xy2 in BlockInfo.blockTypeShapes[(int)BlockType.CONTROL].GetJoins(xy.x, xy.y, 0)) {
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
                        foreach (XY xy2 in BlockInfo.blockTypeShapes[(int)chosen].GetJoins(xy.x, xy.y, r))
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

        List<Vector2> wheelz = new List<Vector2>();
        IDictionary<XY, System.Tuple<BlockType, int>> map = new Dictionary<XY, System.Tuple<BlockType, int>>();

        int c = 0;
        foreach (XY xy in blockGraph.GetBlockPositions()) {
            if (c == 0) {
                c = 2;
                wheelz.Add(new Vector2(xy.x, xy.y) * 1.5f);
            }
            else c--;

            BlockGraph.BlockData data = blockGraph.GetBlockData(xy);
            map[xy] = new System.Tuple<BlockType, int>(data.type, data.rot);
        }

        return new Robot(map, new XY(0, 0), wheelz);
    }
}
