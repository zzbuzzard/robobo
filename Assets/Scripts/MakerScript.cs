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
        { 4, BlockType.CHAINSAW },
        { 5, BlockType.HOVER }
    };

    BlockType currentBlock = BlockType.METAL;
    //private IDictionary<XY, MakerSceneBlockScript> squares;

    static int squareWidth = 10;
    static int squareHeight = 10;
    MakerSceneBlockScript[,] positions; // 2D array

    //XY[,] posOccupiedBy;
    //private static XY nullXY = new XY(-1, -1);

    BlockGraph blockGraph;

    public GameObject squareObj;
    public TextMeshProUGUI errorText;
    public Button goButton;

    // errors: if any are true, then it's invalid
    //public bool noControl { get; private set; }
    //public bool tooManyControl { get; private set; }
    //public bool disconnected { get; private set; }
    //public bool overlaps { get; private set; }

    private void GenerateSquares()
    {
        blockGraph = new BlockGraph();
        positions = new MakerSceneBlockScript[squareWidth, squareHeight];
        //posOccupiedBy = new XY[squareWidth, squareHeight];

        for (int i=0; i<squareWidth; i++)
        {
            for (int j=0; j<squareHeight; j++)
            {
                GameObject obj = Instantiate(squareObj, new Vector2(i - squareWidth / 2.0f, j - squareHeight / 2.0f), Quaternion.identity);
                MakerSceneBlockScript square = obj.GetComponent<MakerSceneBlockScript>();
                square.pos = new XY(i, j);
                square.maker = this;

                positions[i, j] = square;
                //posOccupiedBy[i, j] = nullXY;
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
            if (blockGraph.IsOccupied(pos))
            {
                XY realPos = blockGraph.GetOccupiedBy(pos);
                SpaceClicked(positions[realPos.x, realPos.y]);
                return;
            }

            space.SetBlock(currentBlock);
            blockGraph.AddBlock(pos, 0, currentBlock);
        }
        else
        {
            int r = space.GetRotation();
            if (r == 3)
            {
                space.ClearBlock();
                blockGraph.RemoveAt(space.pos);
            }
            else
            {
                space.Rotate();
                blockGraph.RotateAt(space.pos);
            }
        }

        CheckIssues();
    }

    private void CheckIssues()
    {
        for (int i = 0; i < squareWidth; i++)
        {
            for (int j = 0; j < squareHeight; j++)
            {
                if (positions[i, j].IsEmpty()) continue;
                positions[i, j].MarkReachable(true);
            }
        }

        foreach (XY xy in blockGraph.Unreachable())
        {
            positions[xy.x, xy.y].MarkReachable(false);
        }

        foreach (XY xy in blockGraph.GetOverlaps())
        {
            positions[xy.x, xy.y].MarkReachable(false);
        }

        bool valid = true;

        if (!blockGraph.IsConnected())
        {
            valid = false;
            // TODO: Error message
        }

        if (blockGraph.NumberOfControlBlocks() != 1)
        {
            valid = false;
            // TODO: Error message
        }

        if (blockGraph.HasOverlaps())
        {
            valid = false;
            // TODO: Error message
        }

        if (!valid)
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
        if (blockGraph==null || !blockGraph.IsValidRobot()) return;
        Controller.playerRobot = GetRobot();
        SceneManager.LoadScene("ControlledScene");
    }

    public Robot GetRobot()
    {
        IDictionary<XY, BlockType> blockType = new Dictionary<XY, BlockType>();
        IDictionary<XY, int> rotations = new Dictionary<XY, int>();

        for (int i = 0; i < squareWidth; i++)
        {
            for (int j = 0; j < squareHeight; j++)
            {
                if (!positions[i, j].IsEmpty())
                {
                    XY xy = new XY(i, j);

                    blockType[xy] = positions[i, j].GetBlock();
                    rotations[xy] = positions[i, j].GetRotation();                    
                }
            }
        }

        // XY -> (type, int)
        // center XY
        // List (Vector2) wheels

        return new Robot(blockType, rotations);
    }
}
