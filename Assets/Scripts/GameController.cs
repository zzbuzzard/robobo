using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game controller: 
//  Controls the current game, e.g. by loading in the correct robots
public class GameController : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;

    // Load the game
    private void Awake()
    {
        // Spawn player
        Robot chosenRobot = Controller.playerRobot;

        GameObject playerObj = Instantiate(player, Vector2.zero, Quaternion.identity);
        playerObj.GetComponent<RobotScript>().LoadRobot(chosenRobot);

        SpawnEnemy(chosenRobot);
    }

    private void SpawnEnemy(Robot r)
    {
        float angle = Random.Range(0.0f, Mathf.PI * 2);

        GameObject enemyObj = Instantiate(enemy, 15.0f * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), Quaternion.identity);
        enemyObj.GetComponent<RobotScript>().LoadRobot(r);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int blockz = Random.Range(3, 8);
            int weaponz = Random.Range(1, blockz);
            Robot r = Robot.GenerateRandomRobot(blockz, weaponz);

            SpawnEnemy(r);
        }
    }
}
