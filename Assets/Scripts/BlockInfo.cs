using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

public enum BlockType
{
    CONTROL,
    METAL,
    SPIKE,
    PISTON,
    CHAINSAW,
    HOVER,

    // Keep this one last
    NONE,
}

// Stores and handles the shape and connections of a block
public class BlockShape
{
    List<XY> pos; // (0, 0) must be in here, as it's the center
    List<XY> join;

    public BlockShape(List<XY> pos, List<XY> join) {
        this.pos = pos;
        this.join = join;
    }

    public List<XY> GetOccupiedPositions(int cx, int cy, int rotation) {
        List<XY> ans = new List<XY>();
        XY offset = new XY(cx, cy);
        foreach (XY xy in pos) {
            XY xy2 = Util.RotateBy(xy, rotation);
            xy2 += offset;
            ans.Add(xy2);
        }
        return ans;
    }

    public List<XY> GetJoins(int cx, int cy, int rotation) {
        List<XY> ans = new List<XY>();
        XY offset = new XY(cx, cy);
        foreach (XY xy in join)
        {
            XY xy2 = Util.RotateBy(xy, rotation);
            xy2 += offset;
            ans.Add(xy2);
        }
        return ans;
    }

    public bool ContainsPoint(int x, int y, int cx, int cy, int rotation) {
        XY point = new XY(x, y);
        return GetOccupiedPositions(cx, cy, rotation).Contains(point);
    }

    public bool JoinsToPoint(int x, int y, int cx, int cy, int rotation) {
        XY point = new XY(x, y);
        return GetJoins(cx, cy, rotation).Contains(point);
    }

    // Static method: tells us whether two blocks are connected
    public static bool IsConnected(int x1, int y1, int r1, BlockType s1,
        int x2, int y2, int r2, BlockType s2) {
        BlockShape shape1 = BlockInfo.blockTypeShapes[(int)s1];
        BlockShape shape2 = BlockInfo.blockTypeShapes[(int)s2];
        return IsConnected(x1, y1, r1, shape1, x2, y2, r2, shape2);
    }

    public static bool IsConnected(int x1, int y1, int r1, BlockShape s1, 
        int x2, int y2, int r2, BlockShape s2)
    {
        // Connected iff s1 joins to something in s2
        // And s2 joins to something in s1

        List<XY> joins1 = s1.GetJoins(x1, y1, r1);
        List<XY> joins2 = s2.GetJoins(x2, y2, r2);

        List<XY> pos1 = s1.GetOccupiedPositions(x1, y1, r1);
        List<XY> pos2 = s2.GetOccupiedPositions(x2, y2, r2);

        HashSet<XY> pos1s = new HashSet<XY>(pos1);
        HashSet<XY> pos2s = new HashSet<XY>(pos2);

        bool c1_2 = false,
             c2_1 = false;

        // check if A joins to B
        foreach (XY xy in joins1) {
            if (pos2s.Contains(xy)) {
                c1_2 = true;
                break;
            }
        }

        // check if B joins to A
        foreach (XY xy in joins2) {
            if (pos1s.Contains(xy)) {
                c2_1 = true;
                break;
            }
        }

        return c1_2 && c2_1;
    }

    public static BlockShape OneByOne() {
        List<XY> a = new List<XY>(); a.Add(new XY(0, 0));
        List<XY> b = new List<XY>();
        b.Add(new XY(1, 0));
        b.Add(new XY(-1, 0));
        b.Add(new XY(0, 1));
        b.Add(new XY(0, -1));
        return new BlockShape(a, b);
    }
}

public class BlockInfo
{
    public static BlockType[] weapons = { BlockType.SPIKE, BlockType.PISTON, BlockType.CHAINSAW };
    public static BlockType[] wheels  = { BlockType.HOVER };

    // relative to Resources/Prefabs/BlockPrefabs
    public static string[] blockTypePaths = {
        "control_block",
        "metal_block",
        "spike_block",
        "piston_block",
        "chainsaw_block",
        "hover_block",
    };
    public static BlockShape[] blockTypeShapes = {
        BlockShape.OneByOne(),
        BlockShape.OneByOne(),

        // Spike - cant anywhere but behind
        new BlockShape(new List<XY>(){ new XY(0, 0) },
            new List<XY>(){ new XY(0, -1) }),

        // Piston - cant go in front
        new BlockShape(new List<XY>(){ new XY(0, 0) },
            new List<XY>(){ new XY(0, -1), new XY(1, 0), new XY(-1, 0) }),

        // Chainsaw
        new BlockShape(new List<XY>(){ new XY(0, 0), new XY(0, 1) },
            new List<XY>(){ new XY(0, -1), new XY(-1, 0), new XY(1, 0) }),

        BlockShape.OneByOne(),
    };

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

}
