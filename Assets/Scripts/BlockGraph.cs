using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = UnityEngine.Vector2Int;

// Summary:
// Stores blocks in a graph, allowing for adjacency lookups, connectedness queries, etc.

public class BlockGraph
{
    public struct BlockData {
        public XY pos;
        public int rot;
        public BlockType type;

        public BlockData(XY pos, int rot, BlockType type) {
            this.pos = pos; this.rot = rot; this.type = type;
        }
    }

    IDictionary<XY, XY> occupiedByMap;    // Map of position -> root
    IDictionary<XY, List<XY>> adjacent;   // Should only contain central (XY)s
    IDictionary<XY, BlockData> blockDataMap; // Maps control position -> rotation, type

    ISet<XY> controlSet;
    ISet<XY> overlappingBlocks;

    public BlockGraph() {
        adjacent = new Dictionary<XY, List<XY>>();

        occupiedByMap = new Dictionary<XY, XY>();
        blockDataMap = new Dictionary<XY, BlockData>();
        controlSet = new HashSet<XY>();
        overlappingBlocks = new HashSet<XY>();
    }

    public BlockGraph(Robot robot) {
        adjacent = new Dictionary<XY, List<XY>>();

        occupiedByMap = new Dictionary<XY, XY>();
        blockDataMap = new Dictionary<XY, BlockData>();
        controlSet = new HashSet<XY>();
        overlappingBlocks = new HashSet<XY>();

        foreach (XY xy in robot.blockTypes.Keys) {
            BlockType type = robot.blockTypes[xy];
            int rot = robot.rotations[xy];

            AddBlock(xy, rot, type);
        }
    }

    // Set the occupiedByMap for this thing, checking for overlaps
    private void SetOccupied(XY root) {
        BlockData data = blockDataMap[root];
        List<XY> occupied = BlockInfo.blockInfos[(int)data.type].shape.GetOccupiedPositions(root.x, root.y, data.rot);
        foreach(XY oc in occupied)
        {
            if (occupiedByMap.ContainsKey(oc)) {
                overlappingBlocks.Add(root);
                overlappingBlocks.Add(occupiedByMap[oc]);
            }
            occupiedByMap[oc] = root;
        }
    }

    private void RecalculateAllOccupied()
    {
        overlappingBlocks.Clear();
        occupiedByMap.Clear();
        foreach (XY xy in blockDataMap.Keys)
        {
            SetOccupied(xy);
        }
    }
    
    // PUBLIC FUNCTIONS:

    // The conditions
    public int NumberOfControlBlocks()
    {
        return controlSet.Count;
    }
    
    public ref readonly ISet<XY> GetOverlaps()
    {
        return ref overlappingBlocks;
    }

    public bool HasOverlaps()
    {
        return GetOverlaps().Count > 0;
    }

    public bool IsConnected()
    {
        return Unreachable().Count == 0;
    }

    public bool IsValidRobot()
    {
        return !HasOverlaps() && NumberOfControlBlocks() == 1 && IsConnected();
    }

    public List<XY> GetAdjacent(XY xy)
    {
        if (!adjacent.ContainsKey(xy))
        {
            Debug.LogWarning("Requesting adjacent to " + xy + " but it's not in the graph.");
            return new List<XY>();
        }
        return adjacent[xy];
    }

    // Return all those unreachable from the root
    public List<XY> Unreachable()
    {
        ISet<XY> seen = new HashSet<XY>();
        Queue<XY> queue = new Queue<XY>();

        foreach (XY xy in controlSet)
        {
            seen.Add(xy);
            queue.Enqueue(xy);

            while (queue.Count > 0)
            {
                XY a = queue.Dequeue();
                foreach (XY adj in adjacent[a])
                {
                    if (!seen.Contains(adj))
                    {
                        seen.Add(adj);
                        queue.Enqueue(adj);
                    }
                }
            }
        }

        List<XY> notSeen = new List<XY>();
        foreach (XY xy in blockDataMap.Keys)
        {
            if (!seen.Contains(xy))
                notSeen.Add(xy);
        }

        return notSeen;
    }

    // Removes all unreachable, and returns their positions
    public List<XY> RemoveAllUnreachable()
    {
        List<XY> unreachable = Unreachable();
        foreach (XY xy in unreachable) {
            RemoveAtInternal(xy);
        }
        RecalculateAllOccupied();
        return unreachable;
    }

    public bool IsOccupied(XY xy)
    {
        return occupiedByMap.ContainsKey(xy);
    }
    public XY GetOccupiedBy(XY xy)
    {
        return occupiedByMap[xy];
    }

    public ICollection<XY> GetBlockPositions()
    {
        return blockDataMap.Keys;
    }

    public BlockData GetBlockData(XY xy)
    {
        return blockDataMap[xy];
    }

    // Can we fit this in here? And will it be joined to something?
    // (Returns true if empty)
    public bool CanPlace(XY xy, int rotation, BlockType type)
    {
        if (blockDataMap.Count == 0) return true;

        List<XY> occ = BlockInfo.blockInfos[(int)type].shape.GetOccupiedPositions(xy.x, xy.y, rotation);
        List<XY> joins = BlockInfo.blockInfos[(int)type].shape.GetJoins(xy.x, xy.y, rotation);

        // Check positions are free
        foreach (XY p in occ) {
            if (occupiedByMap.ContainsKey(p)) return false;
        }

        HashSet<XY> seen = new HashSet<XY>();
        bool found = false;
        foreach (XY j in joins)
        {
            if (!occupiedByMap.ContainsKey(j)) continue;
            XY other = occupiedByMap[j];
            if (seen.Contains(other)) continue;
            seen.Add(other);

            int r2 = blockDataMap[other].rot;
            BlockType t2 = blockDataMap[other].type;

            if (BlockShape.IsConnected(xy.x, xy.y, rotation, type, other.x, other.y, r2, t2))
            {
                found = true;
                break;
            }
        }

        if (!found) return false;

        return true;
    }

    // Insert new block
    public void AddBlock(XY xy, int rotation, BlockType type)
    {
        if (blockDataMap.ContainsKey(xy))
        {
            Debug.LogError("Robot contains blocks with overlapping bases");
            return;
        }

        blockDataMap[xy] = new BlockData(xy, rotation, type);
        SetOccupied(xy);

        if (type == BlockType.CONTROL)
        {
            controlSet.Add(xy);
        }

        BlockShape myShape = BlockInfo.blockInfos[(int)type].shape;

        HashSet<XY> considered = new HashSet<XY>();

        adjacent[xy] = new List<XY>();

        // Check for adjacency
        foreach (XY joinPos in myShape.GetJoins(xy.x, xy.y, rotation))
        {
            if (!occupiedByMap.ContainsKey(joinPos)) continue;

            XY other = occupiedByMap[joinPos];
            if (considered.Contains(other)) continue;
            considered.Add(other);

            int r2 = blockDataMap[other].rot;
            BlockType t2 = blockDataMap[other].type;

            if (BlockShape.IsConnected(xy.x, xy.y, rotation, type, other.x, other.y, r2, t2))
            {
                adjacent[other].Add(xy);
                adjacent[xy].Add(other);
            }
        }
    }

    public void RotateAt(XY xy)
    {
        if (!blockDataMap.ContainsKey(xy))
        {
            Debug.LogError("Trying to ROTATE block at " + xy + " but there is no such block.");
            return;
        }
        BlockData data = blockDataMap[xy];
        RemoveAt(xy);
        AddBlock(xy, (data.rot + 1) % 4, data.type);
    }

    // Remove and leave in a valid state
    public void RemoveAt(XY xy)
    {
        if (!blockDataMap.ContainsKey(xy))
        {
            Debug.LogError("Trying to remove block at " + xy + " but there is no such block.");
            return;
        }
        RemoveAtInternal(xy);
        RecalculateAllOccupied();
    }

    // Removes a node from all lists, but does not call RecalculateAllOccupied()
    // Should only be used internally for batch removals.
    private void RemoveAtInternal(XY xy)
    {
        List<XY> myAdj = adjacent[xy];
        for (int i = 0; i < myAdj.Count; i++) {
            adjacent[myAdj[i]].Remove(xy);
        }

        adjacent.Remove(xy);
        blockDataMap.Remove(xy);
        if (controlSet.Contains(xy)) controlSet.Remove(xy);
    }

    public void PrintData()
    {
        Debug.Log("Contained blocks: " + blockDataMap.Keys.Count);
        foreach (XY xy in adjacent.Keys)
        {
            Debug.Log("Block of type " + blockDataMap[xy].type.ToString() +
                " at position " + xy + ": " + adjacent[xy].ToString());
        }
    }
}
