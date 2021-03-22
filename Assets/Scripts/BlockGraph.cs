using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = System.Tuple<int, int>;

public class BlockGraph
{
    IDictionary<XY, Block> posMap;
    XY controlPos;

    // up, left, down, right
    public static int[] ox = { 0, -1, 0, 1 },
           oy = { 1, 0, -1, 0 };

    // no clue what this documentation shit is
    /// <summary>
    ///  blocks[control] should be the Control block
    /// </summary>
    /// <param name="blocks"></param> the blocks
    /// <param name="center"></param> the index of the control block
    public BlockGraph(List<Block> blocks, int control)
    {
        posMap = new Dictionary<XY, Block>();
        foreach (Block b in blocks)
        {
            posMap[new XY(b.x, b.y)] = b;
        }
        controlPos = new XY(blocks[control].x, blocks[control].y);
    }

    // BFS from Control to determine which nodes remain reachable
    public List<Block> KillComponent(Block a)
    {
        XY pos = new XY(a.x, a.y);

        // Already been destroyed
        if (!posMap.ContainsKey(pos))
            return new List<Block>();

        posMap.Remove(pos);

        // Wow. This isn't the same as pos == controlPos, for whatever the fuck reason. 
        // Theirs is referential equality only??
        if (pos.Item1 == controlPos.Item1 && pos.Item2 == controlPos.Item2)
        {
            List<Block> deleted = new List<Block>();
            foreach (XY x in posMap.Keys)
            {
                deleted.Add(posMap[x]);
            }
            posMap.Clear();
            return deleted;
        }

        Queue<XY> queue = new Queue<XY>();
        ISet<XY> seen = new HashSet<XY>();

        queue.Enqueue(controlPos);
        seen.Add(controlPos);

        // BFS
        while (queue.Count > 0)
        {
            XY front = queue.Dequeue();

            // Four neighbours (right down left up)
            for (int i=0; i<4; i++)
            {
                XY next = new XY(front.Item1 + ox[i], front.Item2 + oy[i]);

                // Check if it has a block here, and the block is unseen
                if (posMap.ContainsKey(next) && !seen.Contains(next))
                {
                    seen.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        // Must do in two passes, as you cannot remove from dict while iterating through it
        List<XY> unreachable = new List<XY>();
        List<Block> unreachableBlocks = new List<Block>();
        foreach (XY x in posMap.Keys)
        {
            if (!seen.Contains(x))
                unreachable.Add(x);
        }

        foreach (XY x in unreachable)
        {
            unreachableBlocks.Add(posMap[x]);
            posMap.Remove(x);
        }

        return unreachableBlocks;
    }
}
