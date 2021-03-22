using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using XY = System.Tuple<int, int>;

// Script to manage the making of a robot

// UI:
//  - Blocks where you can place
//  - Click at bottom to select the thing you want
//  - Then click a block, click again to rotate

public class MakerScript : MonoBehaviour
{
    // Maps a button ID to the block type it represents
    static IDictionary<int, Robot.BlockType> buttonToBlock = new Dictionary<int, Robot.BlockType>()
    {
        { 0, Robot.BlockType.CONTROL },
        { 1, Robot.BlockType.METAL },
        { 2, Robot.BlockType.SPIKE },
        { 3, Robot.BlockType.PISTON },
        { 4, Robot.BlockType.CHAINSAW }
    };

    Robot.BlockType currentBlock = Robot.BlockType.METAL;

    private IDictionary<XY, MakerSceneBlockScript> squares;
    public GameObject squareObj;

    void Start()
    {
        squares = new Dictionary<XY, MakerSceneBlockScript>();
        MakeSquare(new XY(0, 0));
    }

    private void MakeSquare(XY pos)
    {
        if (squares.ContainsKey(pos))
        {
            Debug.LogWarning("Trying to create a square at a position which is occupied");
            return;
        }

        GameObject obj = Instantiate(squareObj, new Vector3(pos.Item1, pos.Item2, 0.0f), Quaternion.identity);
        MakerSceneBlockScript square = obj.GetComponent<MakerSceneBlockScript>();
        square.pos = pos;
        square.maker = this;
        squares[pos] = square;
    }

    private void DelSquare(XY pos)
    {
        if (!squares.ContainsKey(pos))
        {
            Debug.LogWarning("Trying to delete a square which didn't exist");
            return;
        }
        MakerSceneBlockScript m = squares[pos];
        Destroy(m.gameObject);
        squares.Remove(pos);
    }

    public void SpaceClicked(MakerSceneBlockScript space)
    {
        if (space.IsEmpty())
        {
            space.SetBlock(currentBlock);
        }
        else
        {
            int r = space.GetRotation();
            if (r == 3) space.ClearBlock();
            else space.Rotate();
        }

        RecheckBounds();
    }

    // Checks if valid (connected to start)
    // Also checks if we have a control
    // And places MakerSceneBlocks
    private void RecheckBounds()
    {
        // TODO: Search from start

        print("Rechecking bounds");

        // bfs from each which isn't empty
        Queue<XY> queue = new Queue<XY>();
        ISet<XY> seen = new HashSet<XY>();
        
        foreach (XY x in squares.Keys)
        {
            if (!squares[x].IsEmpty() && !seen.Contains(x))
            {
                queue.Enqueue(x);
                seen.Add(x);

                while (queue.Count > 0)
                {
                    print("Queue size: " + queue.Count);

                    XY a = queue.Dequeue();
                    for (int i=0; i<4; i++)
                    {
                        XY b = new XY(a.Item1 + BlockGraph.ox[i], a.Item2 + BlockGraph.oy[i]);
                        if (squares.ContainsKey(b) && !seen.Contains(b) && !squares[b].IsEmpty())
                        {
                            seen.Add(b);
                            queue.Enqueue(b);
                        }
                    }
                }
            }
        }

        foreach (XY x in seen)
        {
            for (int i=0; i<4; i++)
            {
                XY y = new XY(x.Item1 + BlockGraph.ox[i], x.Item2 + BlockGraph.oy[i]);
                if (!squares.ContainsKey(y))
                {
                    MakeSquare(y);
                }
            }
        }
    }

    public void ButtonClicked(int button)
    {
        if (!buttonToBlock.ContainsKey(button))
        {
            Debug.LogError("Button " + button + " has no associated block");
            return;
        }
        currentBlock = buttonToBlock[button];
    }

    public void StartClicked()
    {
        Controller.playerRobot = GetRobot();
        SceneManager.LoadScene("ControlledScene");
    }

    public Robot GetRobot()
    {
        IDictionary<XY, System.Tuple<Robot.BlockType, int>> dict = new Dictionary<XY, System.Tuple<Robot.BlockType, int>>();
        XY center = new XY(0, 0);

        int tilWheel = 0;
        List<Vector2> wheelz = new List<Vector2>();
        wheelz.Add(new Vector2(-1.5f, 0.0f));
        wheelz.Add(new Vector2(1.5f, 0.0f));

        foreach (XY x in squares.Keys)
        {
            if (!squares[x].IsEmpty())
            {
                dict[x] = new System.Tuple<Robot.BlockType, int>(
                    squares[x].GetBlock(), squares[x].GetRotation()
                    );

                if (squares[x].GetBlock() == Robot.BlockType.CONTROL) center = x;

                if (tilWheel == 0)
                {
                    tilWheel = 2;
                    wheelz.Add(new Vector2(x.Item1, x.Item2) * 1.5f);
                }
                else tilWheel--;
            }
        }

        // XY -> (type, int)
        // center XY
        // List (Vector2) wheels

        return new Robot(dict, center, wheelz);
    }
}
