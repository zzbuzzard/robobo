using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Important and boring stuff
public class Controller
{
    // Runs when the game is loaded, before the first scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialise()
    {
        Robot.LoadBlockTypePrefabs();
    }
}
