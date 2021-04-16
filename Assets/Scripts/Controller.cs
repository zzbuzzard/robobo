using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Important and boring stuff
public class Controller
{
    public static Robot playerRobot;
    public static int budget = 200; // TODO

    // true only when client + playing offline
    public static bool isLocalGame = false;

    // Runs when the game is loaded, before the first scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialise()
    {
        BlockInfo.LoadBlockTypePrefabs();

#if UNITY_SERVER
#else
        SparkScript.sparkPrefab = Resources.Load<GameObject>("Prefabs/sparks");
        playerRobot = Robot.LoadRobotFromName(PlayerPrefs.GetString("PREF_ROBOT", "default"));
#endif
    }
}
