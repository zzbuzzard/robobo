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
    THRUSTER,

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
        new BlockInfo("control_block", BlockShape.OneByOne(), 0, WheelType.NONE, "control"),
        new BlockInfo("metal_block", BlockShape.OneByOne(), 0, WheelType.NONE, "metal"),
        new BlockInfo("spike_block",
            new BlockShape(new List<XY>(){ new XY(0, 0) },
            new List<XY>(){ new XY(0, -1) }),
            3, WheelType.NONE, "spike"),
        new BlockInfo("piston_block",
            new BlockShape(new List<XY>(){ new XY(0, 0), new XY(0, 1) },
            new List<XY>(){ new XY(0, -1), new XY(1, 0), new XY(-1, 0) }),
            3, WheelType.NONE, "piston_image"),
        new BlockInfo("chainsaw_block",
            new BlockShape(new List<XY>(){ new XY(0, 0), new XY(0, 1) },
            new List<XY>(){ new XY(0, -1), new XY(-1, 0), new XY(1, 0) }),
            3, WheelType.NONE, "Chainsaw1"),
        new BlockInfo("hover_block", BlockShape.OneByOne(), 0, WheelType.HOVER, "Hover"),
        new BlockInfo("track_block",
            new BlockShape(new List<XY>(){ new XY(-1, 0), new XY(0, 0), new XY(1, 0) },
            new List<XY>(){ new XY(-1, -1), new XY(0, -1), new XY(1, -1), new XY(-1, 1), new XY(0, 1), new XY(1, 1) }),
            1, WheelType.TRACK, "Tracks1"),
        new BlockInfo("thrust_block", BlockShape.OneByOne(), 3, WheelType.NONE, "thrust"),
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

            blockInfos[i].showSprite = Resources.Load<Sprite>("Sprites/" + blockInfos[i].spritePath);
            if (blockInfos[i].showSprite == null)
            {
                Debug.LogWarning("Warning: Block \"" + blockInfos[i].path + "\" could not find sprite.");
            }

        }
    }


    // Non-static stuff

    private string path; // Path to prefab
    public GameObject prefab { get; private set; }

    private string spritePath; // The sprite which shows up on the build screens
    public Sprite showSprite { get; private set; }

    public BlockShape shape { get; private set; }
    public int maxRot { get; private set; } // 0 means not rotatable, 1 means hor/ver, 3 means full circle
    public WheelType wheelType { get; private set; }

    public BlockInfo(string path, BlockShape shape, int maxRot, WheelType wheelType, string spritePath)
    {
        this.path = path;
        this.shape = shape;
        this.maxRot = maxRot;
        this.wheelType = wheelType;
        this.spritePath = spritePath;

        this.prefab = null; // Set in LoadBlockTypePrefabs
        this.showSprite = null;
    }
}
