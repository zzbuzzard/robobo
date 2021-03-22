using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game controller: 
//  Controls the current game, e.g. by loading in the correct robots
public class GameController : MonoBehaviour
{
    public GameObject player;

    // Load the game
    private void Awake()
    {
        // Spawn player
        Robot chosenRobot = Controller.playerRobot;

        GameObject playerObj = Instantiate(player, Vector2.zero, Quaternion.identity);
        playerObj.GetComponent<MovementScript>().LoadRobot(chosenRobot);
    }
}
