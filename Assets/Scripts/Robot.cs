using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = System.Tuple<int, int>;

// A class to store a robot (it's blocks, stats, etc...)
public class Robot
{
    public enum BlockType
    {
        CONTROL,
        METAL,
        SPIKE,
        PISTON,
        CHAINSAW,

        // Keep this one last
        NONE,
    }

    public static Robot.BlockType[] weapons = { Robot.BlockType.SPIKE, Robot.BlockType.PISTON, Robot.BlockType.CHAINSAW };

    // relative to Resources/Prefabs/BlockPrefabs
    public static string[] blockTypePaths = { "control_block", "metal_block", "spike_block", "piston_block", "chainsaw_block" };
    public static GameObject[] blockTypePrefabs;
    public static void LoadBlockTypePrefabs()
    {
        int N = blockTypePaths.Length;
        blockTypePrefabs = new GameObject[N];
        for (int i = 0; i < N; i++)
        {
            blockTypePrefabs[i] = Resources.Load<GameObject>("Prefabs/BlockPrefabs/" + blockTypePaths[i] + " Variant");
            if (blockTypePrefabs[i] == null)
            {
                Debug.LogWarning("Warning: Block \"" + blockTypePaths[i] + "\" not found.");
            }
        }
    }

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
        IDictionary<XY, System.Tuple<BlockType, int>> blocks = new Dictionary<XY, System.Tuple<BlockType, int>>();
        blocks[new XY(0, 0)] = new System.Tuple<BlockType, int>(BlockType.CONTROL, 0);

        IList<XY> spaces = new List<XY>();
        for (int i = 0; i < 4; i++) spaces.Add(new XY(BlockGraph.ox[i], BlockGraph.oy[i]));

        for (int _=0; _<blockNum-1; _++)
        {
            while (spaces.Count > 0)
            {
                int index = Random.Range(0, spaces.Count);
                XY xy = spaces[index];
                spaces.RemoveAt(index);

                bool placed = false;

                if (!blocks.ContainsKey(xy))
                {
                    blocks[xy] = new System.Tuple<BlockType, int>(BlockType.METAL, 0);
                    placed = true;

                    for (int i = 0; i < 4; i++)
                    {
                        XY xy2 = new XY(xy.Item1 + BlockGraph.ox[i], xy.Item2 + BlockGraph.oy[i]);
                        if (!spaces.Contains(xy2) && !blocks.ContainsKey(xy2))
                        {
                            spaces.Add(xy2);
                        }
                    }
                }

                if (placed) break;
            }
        }

        for (int _ = 0; _ < weaponNum; _++)
        {
            while(spaces.Count > 0)
            {
                int index = Random.Range(0, spaces.Count - 1);
                XY xy = spaces[index];
                spaces.RemoveAt(index);

                if (!blocks.ContainsKey(xy))
                {
                    BlockType chosen = weapons[Random.Range(0, weapons.Length)];

                    int ind = 2;
                    for (int i=0; i<4; i++)
                    {
                        XY xy2 = new XY(xy.Item1 + BlockGraph.ox[i], xy.Item2 + BlockGraph.oy[i]);
                        if (blocks.ContainsKey(xy2))
                        {
                            ind = i;
                            break;
                        }
                    }
                    ind = (2 + ind) % 4;
                    blocks[xy] = new System.Tuple<BlockType, int>(chosen, ind);

                    break;
                }
            }
        }

        List<Vector2> wheelz = new List<Vector2>();
        int c = 0;
        foreach (XY x in blocks.Keys) {
            if (c == 0) {
                c = 2;
                wheelz.Add(new Vector2(x.Item1, x.Item2) * 1.5f);
            }
            else c--;
        }

        return new Robot(blocks, new XY(0, 0), wheelz);
    }
}
