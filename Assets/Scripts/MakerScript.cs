using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using XY = UnityEngine.Vector2Int;

// Script to manage the making of a robot

// TODO:
//  - Drag and drop: auto snaps to grid, including auto snapping rotation to nearest boi

public class MakerScript : MonoBehaviour
{
    public static string RobotName { get; private set; } = "default";
    private static Robot unsavedRobot = null;
    public static void LoadSavedRobot(string name)
    {
        RobotName = name;
        unsavedRobot = null;
    }
    public static void LoadUnsavedRobot(string name, Robot r)
    {
        RobotName = name;
        unsavedRobot = r;
    }

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

        // Back from testing - load unsaved robot
        if (unsavedRobot != null)
        {
            LoadRobot(unsavedRobot);
        }
        else
        {
            // Loaded from screen, either new or not
            Robot edit = Robot.LoadRobotFromName(RobotName);
            if (edit != null)
            {
                LoadRobot(edit);
            }
        }

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
            int maxRot = BlockInfo.blockInfos[(int)space.GetBlock()].maxRot;

            int r = space.GetRotation();

            if (r == maxRot)
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

        string err = "";

        if (!blockGraph.IsConnected())
        {
            valid = false;
            err += "Blocks don't connect\n";
        }

        if (blockGraph.NumberOfControlBlocks() != 1)
        {
            valid = false;
            if (blockGraph.NumberOfControlBlocks() == 0)
                err += "Missing control block\n";
            else
                err += "Too many control blocks\n";
        }

        if (blockGraph.HasOverlaps())
        {
            valid = false;
            err += "Overlapping blocks\n";
        }

        if (!valid)
        {
            errorText.SetText(err.Substring(0, err.Length - 1));
            goButton.interactable = false;
        }
        else
        {
            errorText.SetText("");
            goButton.interactable = true;
        }
    }

    public void ButtonClicked(BlockType block)
    {
        currentBlock = block;
    }

    public void StartClicked()
    {
        if (blockGraph == null || !blockGraph.IsValidRobot()) return;
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

    private void ResetGrid()
    {
        blockGraph = new BlockGraph();
        for (int i = 0; i < squareWidth; i++)
        {
            for (int j = 0; j < squareHeight; j++)
            {
                positions[i, j].ClearBlock();
            }
        }
        CheckIssues();
    }

    private void LoadRobot(Robot r)
    {
        ResetGrid();

        IDictionary<XY, BlockType> types = r.blockTypes;
        IDictionary<XY, int> rots = r.rotations;

        foreach (XY xy in types.Keys)
        {
            BlockType type = types[xy];
            int rot = rots[xy];

            positions[xy.x, xy.y].SetBlock(type);
            positions[xy.x, xy.y].SetRotation(rot);

            blockGraph.AddBlock(xy, rot, type);
        }

        CheckIssues();
    }

    //public void LoadClicked()
    //{
    //    Robot r = Robot.LoadRobotFromName(robotName);
    //    if (r != null)
    //        LoadRobot(r);
    //}

    public void BackClicked()
    {
        SceneManager.LoadScene("SelectRobot");
    }

    public void SaveClicked()
    {
        Robot r = GetRobot();
        Robot.SaveRobotToFile(r, RobotName);
    }
}
