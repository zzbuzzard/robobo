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
    public XY center;
    public List<Vector2> wheels;

    public Robot(IDictionary<XY, BlockType> blocks, XY center, List<Vector2> wheels)
    {
        this.blocks = blocks;
        this.center = center;
        this.wheels = wheels;
    }
}
