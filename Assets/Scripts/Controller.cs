using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = System.Tuple<int, int>;

// Important and boring stuff
public class Controller
{
    // The player's robot
    public static Robot playerRobot;

    // Runs when the game is loaded, before the first scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialise()
    {
        Robot.LoadBlockTypePrefabs();

        // Example robot load:
        IDictionary<XY, System.Tuple<Robot.BlockType, int>> dict = new Dictionary<XY, System.Tuple<Robot.BlockType, int>>();

        dict[new XY(0, 0)]  = new System.Tuple<Robot.BlockType, int>(Robot.BlockType.CONTROL, 0);
        dict[new XY(1, 0)]  = new System.Tuple<Robot.BlockType, int>(Robot.BlockType.METAL, 0);
        dict[new XY(-1, 0)] = new System.Tuple<Robot.BlockType, int>(Robot.BlockType.METAL, 0);
        dict[new XY(0, 1)]  = new System.Tuple<Robot.BlockType, int>(Robot.BlockType.PISTON, 0);
        dict[new XY(2, 0)] = new System.Tuple<Robot.BlockType, int>(Robot.BlockType.SPIKE, 3);
        dict[new XY(-2, 0)] = new System.Tuple<Robot.BlockType, int>(Robot.BlockType.SPIKE, 1);

        List<Vector2> wheelz = new List<Vector2>();
        wheelz.Add(new Vector2(-1.5f, 0.0f));
        wheelz.Add(new Vector2(11.5f, 0.0f));

        playerRobot = new Robot(dict, new XY(0, 0), wheelz);
    }
}
