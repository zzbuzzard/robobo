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
        SPIKE
    }
    
    // relative to Resources/Prefabs/BlockPrefabs
    public static string[] blockTypePaths = { "control_block", "metal_block", "spike_block" };
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
}
