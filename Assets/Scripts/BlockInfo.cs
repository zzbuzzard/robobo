using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

// Note: Order matters, as it shows up in the blockInfos list below
public enum BlockType
{
    CONTROL,
    METAL,
    SPIKE,
    PISTON,
    CHAINSAW,
    HOVER,
    TRACK,

    // Keep this one last
    NONE = -1,
}

public enum WheelType
{
    HOVER,
    WHEEL,
    TRACK,

    NONE = -1,
}

public class BlockInfo
{
    public static BlockType[] weapons = { BlockType.SPIKE, BlockType.PISTON, BlockType.CHAINSAW };
    public static BlockType[] wheelBlocks  = { BlockType.HOVER, BlockType.TRACK };
    public static WheelType[] wheelTypes = { WheelType.HOVER, WheelType.WHEEL, WheelType.TRACK }; // wheel types in priority order

    public static BlockInfo[] blockInfos =
    {
        new BlockInfo("control_block", BlockShape.OneByOne(), 0, WheelType.NONE),
        new BlockInfo("metal_block", BlockShape.OneByOne(), 0, WheelType.NONE),
        new BlockInfo("spike_block",
            new BlockShape(new List<XY>(){ new XY(0, 0) },
            new List<XY>(){ new XY(0, -1) }),
            3, WheelType.NONE),
        new BlockInfo("piston_block",
            new BlockShape(new List<XY>(){ new XY(0, 0), new XY(0, 1) },
            new List<XY>(){ new XY(0, -1), new XY(1, 0), new XY(-1, 0) }),
            3, WheelType.NONE),
        new BlockInfo("chainsaw_block",
            new BlockShape(new List<XY>(){ new XY(0, 0), new XY(0, 1) },
            new List<XY>(){ new XY(0, -1), new XY(-1, 0), new XY(1, 0) }),
            3, WheelType.NONE),
        new BlockInfo("hover_block", BlockShape.OneByOne(), 0, WheelType.HOVER),
        new BlockInfo("track_block",
            new BlockShape(new List<XY>(){ new XY(-1, 0), new XY(0, 0), new XY(1, 0) },
            new List<XY>(){ new XY(-1, -1), new XY(0, -1), new XY(1, -1), new XY(-1, 1), new XY(0, 1), new XY(0, 1) }),
            1, WheelType.HOVER),
    };

    public static void LoadBlockTypePrefabs()
    {
        int N = blockInfos.Length;

        for (int i = 0; i < N; i++)
        {
            blockInfos[i].prefab = Resources.Load<GameObject>("Prefabs/BlockPrefabs/" + blockInfos[i].path + " Variant");
            if (blockInfos[i].prefab == null)
            {
                Debug.LogWarning("Warning: Block \"" + blockInfos[i].path + "\" not found.");
            }
        }
    }


    // Non-static stuff

    public string path { get; private set; }
    public BlockShape shape { get; private set; }
    public int maxRot { get; private set; } // 0 means not rotatable, 1 means hor/ver, 3 means full circle
    public WheelType wheelType { get; private set; }
    public GameObject prefab { get; private set; }

    public BlockInfo(string path, BlockShape shape, int maxRot, WheelType wheelType)
    {
        this.path = path;
        this.shape = shape;
        this.maxRot = maxRot;
        this.wheelType = wheelType;
        this.prefab = null; // Set in LoadBlockTypePrefabs
    }
}
