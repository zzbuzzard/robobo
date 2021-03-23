using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using XY = UnityEngine.Vector2Int;

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
    static IDictionary<int, BlockType> buttonToBlock = new Dictionary<int, BlockType>()
    {
        { 0, BlockType.CONTROL },
        { 1, BlockType.METAL },
        { 2, BlockType.SPIKE },
        { 3, BlockType.PISTON },
        { 4, BlockType.CHAINSAW }
    };

    BlockType currentBlock = BlockType.METAL;
    //private IDictionary<XY, MakerSceneBlockScript> squares;

    static int squareWidth = 10;
    static int squareHeight = 10;
    MakerSceneBlockScript[,] positions; // 2D array
    XY[,] posOccupiedBy;
    private static XY nullXY = new XY(-1, -1);

    public GameObject squareObj;
    public TextMeshProUGUI errorText;
    public Button goButton;

    // errors: if any are true, then it's invalid
    public bool noControl { get; private set; }
    public bool tooManyControl { get; private set; }
    public bool disconnected { get; private set; }
    public bool overlaps { get; private set; }

    private void GenerateSquares()
    {
        positions = new MakerSceneBlockScript[squareWidth, squareHeight];
        posOccupiedBy = new XY[squareWidth, squareHeight];

        for (int i=0; i<squareWidth; i++)
        {
            for (int j=0; j<squareHeight; j++)
            {
                GameObject obj = Instantiate(squareObj, new Vector2(i - squareWidth / 2.0f, j - squareHeight / 2.0f), Quaternion.identity);
                MakerSceneBlockScript square = obj.GetComponent<MakerSceneBlockScript>();
                square.pos = new XY(i, j);
                square.maker = this;

                positions[i, j] = square;
                posOccupiedBy[i, j] = nullXY;
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
            XY pos = space.pos;

            // If this space is empty, but it is actually part of another block,
            // Then forward this call to the real MakerSceneBlockScript
            if (posOccupiedBy[pos.x, pos.y] != nullXY) {
                XY realPos = posOccupiedBy[pos.x, pos.y];
                SpaceClicked(positions[realPos.x, realPos.y]);
                return;
            }

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

        // Initialise
        for (int i = 0; i < squareWidth; i++) {
            for (int j = 0; j < squareHeight; j++) {
                nodeSeen[i, j] = false;
                posOccupiedBy[i, j] = nullXY;
            }
        }

        overlaps = false;

        // Find control blocks + overlaps
        for (int i = 0; i < squareWidth; i++) {
            for (int j = 0; j < squareHeight; j++) {
                if (positions[i, j].IsEmpty()) continue;

                // Mark everything we occupy as occupied
                BlockType block = positions[i, j].GetBlock();
                List<XY> occupied = BlockInfo.blockTypeShapes[(int)block].GetOccupiedPositions
                    (i, j, positions[i, j].GetRotation());
                XY myPos = new XY(i, j);
                foreach (XY xy in occupied) {
                    if (posOccupiedBy[xy.x, xy.y] != nullXY) {
                        overlaps = true;

                        // XY other = posOccupiedBy[xy.x, xy.y];
                        // positions[other.x, other.y].MarkOverlap(true);
                    }

                    posOccupiedBy[xy.x, xy.y] = myPos;
                }

                if (block == BlockType.CONTROL)
                {
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
            if (nodeSeen[xy.x, xy.y]) continue;
            queue.Enqueue(xy);
            nodeSeen[xy.x, xy.y] = true;

            while (queue.Count > 0)
            {
                XY a = queue.Dequeue();
                //Debug.Log("Got from queue " + a);

                // Get neighbours
                BlockType myBlock = positions[a.x, a.y].GetBlock();
                int myRot = positions[a.x, a.y].GetRotation();
                List<XY> neighours = BlockInfo.blockTypeShapes[(int)myBlock].GetJoins(a.x, a.y, myRot);

                // Loop through neighbours
                foreach (XY nei in neighours)
                {
                    if (nei.x >= 0 && nei.x < squareWidth && nei.y >= 0 && nei.y < squareHeight
                        && posOccupiedBy[nei.x, nei.y] != nullXY)
                        //&& !nodeSeen[nei.x, b.Item2] && !positions[b.Item1, b.Item2].IsEmpty())
                    {
                        // Get the block occupying this position
                        XY loc = posOccupiedBy[nei.x, nei.y];
                        BlockType neiBlock = positions[loc.x, loc.y].GetBlock();
                        int neiRot = positions[loc.x, loc.y].GetRotation();

                        // Just give up if we've seen it already
                        if (nodeSeen[loc.x, loc.y]) continue;

                        // Determine if the connection does in fact go both ways
                        bool conn = BlockShape.IsConnected(
                            a.x, a.y, myRot, myBlock,
                            loc.x, loc.y, neiRot, neiBlock);

                        if (conn)
                        {
                            //Debug.Log("Found " + loc);
                            nodeSeen[loc.x, loc.y] = true;
                            queue.Enqueue(loc);
                        }
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

        if (disconnected || tooManyControl || noControl || overlaps)
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
        IDictionary<XY, System.Tuple<BlockType, int>> dict = new Dictionary<XY, System.Tuple<BlockType, int>>();
        XY center = new XY(0, 0);

        int tilWheel = 0;
        List<Vector2> wheelz = new List<Vector2>();

        for (int i = 0; i < squareWidth; i++)
        {
            for (int j = 0; j < squareHeight; j++)
            {
                if (!positions[i, j].IsEmpty())
                {
                    XY xy = new XY(i, j);

                    dict[xy] = new System.Tuple<BlockType, int>(
                        positions[i, j].GetBlock(), positions[i, j].GetRotation());

                    if (positions[i, j].GetBlock() == BlockType.CONTROL) center = xy;

                    // TODO: Remove. Better wheel system needed.
                    if (tilWheel == 0)
                    {
                        tilWheel = 2;
                        wheelz.Add(new Vector2(xy.x, xy.y) * 1.5f);
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
