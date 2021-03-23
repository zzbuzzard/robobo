using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using XY = System.Tuple<int, int>;

// Script to manage the making of a robot

// UI:
//  - Blocks where you can place
//  - Click at bottom to select the thing you want
//  - Then click a block, click again to rotate

// TODO:
//  - Allow for NxM blocks
//  - Drag and drop: auto snaps to grid, including auto snapping rotation to nearest boi

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
    //private IDictionary<XY, MakerSceneBlockScript> squares;

    static int squareWidth = 10;
    static int squareHeight = 10;
    MakerSceneBlockScript[,] positions; // 2D array

    public GameObject squareObj;
    public TextMeshProUGUI errorText;
    public Button goButton;

    // errors: if any are true, then it's invalid
    public bool noControl { get; private set; }
    public bool tooManyControl { get; private set; }
    public bool disconnected { get; private set; }

    private void GenerateSquares()
    {
        positions = new MakerSceneBlockScript[squareWidth, squareHeight];
        for (int i=0; i<squareWidth; i++)
        {
            for (int j=0; j<squareHeight; j++)
            {
                GameObject obj = Instantiate(squareObj, new Vector2(i - squareWidth / 2.0f, j - squareHeight / 2.0f), Quaternion.identity);
                MakerSceneBlockScript square = obj.GetComponent<MakerSceneBlockScript>();
                square.pos = new XY(i, j);
                square.maker = this;

                positions[i, j] = square;
            }
        }
    }

    void Start()
    {
        GenerateSquares();
        CheckIssues();

        //squares = new Dictionary<XY, MakerSceneBlockScript>();
        //MakeSquare(new XY(0, 0));
    }

    //private void MakeSquare(XY pos)
    //{
    //    if (squares.ContainsKey(pos))
    //    {
    //        Debug.LogWarning("Trying to create a square at a position which is occupied");
    //        return;
    //    }

    //    squares[pos] = square;
    //}

    //private void DelSquare(XY pos)
    //{
    //    if (!squares.ContainsKey(pos))
    //    {
    //        Debug.LogWarning("Trying to delete a square which didn't exist");
    //        return;
    //    }
    //    MakerSceneBlockScript m = squares[pos];
    //    Destroy(m.gameObject);
    //    squares.Remove(pos);
    //}

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

        CheckIssues();
    }

    private void CheckIssues()
    {
        bool[,] nodeSeen = new bool[squareWidth, squareHeight];
        List<XY> controls = new List<XY>();

        // Find control blocks
        for (int i = 0; i < squareWidth; i++) {
            for (int j = 0; j < squareHeight; j++) {
                nodeSeen[i, j] = false;
                if (positions[i, j] != null && positions[i, j].GetBlock() == Robot.BlockType.CONTROL) {
                    controls.Add(new XY(i, j));
                }
            }
        }

        tooManyControl = (controls.Count > 1);
        noControl = (controls.Count == 0);

        Queue<XY> queue = new Queue<XY>();

        // BFS from each control
        foreach (XY xy in controls)
        {
            if (nodeSeen[xy.Item1, xy.Item2]) continue;
            queue.Enqueue(xy);
            nodeSeen[xy.Item1, xy.Item2] = true;

            while (queue.Count > 0)
            {
                XY a = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    XY b = new XY(a.Item1 + BlockGraph.ox[i], a.Item2 + BlockGraph.oy[i]);

                    if (b.Item1 >= 0 && b.Item1 < squareWidth && b.Item2 >= 0 && b.Item2 < squareHeight
                        && !nodeSeen[b.Item1, b.Item2] && !positions[b.Item1, b.Item2].IsEmpty())
                    {
                        nodeSeen[b.Item1, b.Item2] = true;
                        queue.Enqueue(b);
                    }
                }
            }
        }

        disconnected = false;

        // Check for disconnected nodes, mark those reachable as "reachable"
        for (int i = 0; i < squareWidth; i++) {
            for (int j = 0; j < squareHeight; j++) {
                if (positions[i, j].IsEmpty()) continue;

                positions[i, j].MarkReachable(nodeSeen[i, j]);
                if (!nodeSeen[i, j]) {
                    disconnected = true;
                }
            }
        }

        if (disconnected || tooManyControl || noControl)
        {
            errorText.SetText("Invalid robot, nerd");
            goButton.interactable = false;
        }
        else
        {
            errorText.SetText("");
            goButton.interactable = true;
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
        if (noControl || tooManyControl || disconnected) return;
        Controller.playerRobot = GetRobot();
        SceneManager.LoadScene("ControlledScene");
    }

    public Robot GetRobot()
    {
        IDictionary<XY, System.Tuple<Robot.BlockType, int>> dict = new Dictionary<XY, System.Tuple<Robot.BlockType, int>>();
        XY center = new XY(0, 0);

        int tilWheel = 0;
        List<Vector2> wheelz = new List<Vector2>();

        for (int i = 0; i < squareWidth; i++)
        {
            for (int j = 0; j < squareHeight; j++)
            {
                if (!positions[i, j].IsEmpty())
                {
                    XY x = new XY(i, j);

                    dict[x] = new System.Tuple<Robot.BlockType, int>(
                        positions[i, j].GetBlock(), positions[i, j].GetRotation());

                    if (positions[i, j].GetBlock() == Robot.BlockType.CONTROL) center = x;

                    // TODO: Remove. Better wheel system needed.
                    if (tilWheel == 0)
                    {
                        tilWheel = 2;
                        wheelz.Add(new Vector2(x.Item1, x.Item2) * 1.5f);
                    }
                    else tilWheel--;
                }

            }
        }

        // XY -> (type, int)
        // center XY
        // List (Vector2) wheels

        return new Robot(dict, center, wheelz);
    }
}
