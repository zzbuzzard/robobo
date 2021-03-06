using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Important and boring stuff
public class Controller
{
    // The player's robot
    public static Robot playerRobot;

    // Runs when the game is loaded, before the first scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialise()
    {
        BlockInfo.LoadBlockTypePrefabs();
        SparkScript.sparkPrefab = Resources.Load<GameObject>("Prefabs/sparks");

        playerRobot = Robot.LoadRobotFromName(PlayerPrefs.GetString("PREF_ROBOT", "default"));
    }
}
