using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Game controller: 
//  Controls the current game, e.g. by loading in the correct robots
public class GameController : MonoBehaviour
{
    public bool isOnline = false;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public Vector2 playerSpawn;
    public Vector2 enemySpawn;

    private GameObject player;

    public FixedJoystick left, right;

    // Load the game
    private void Awake()
    {
        if (!isOnline)
        {
            // Spawn player
            Robot chosenRobot = Controller.playerRobot;
            GameObject p = Instantiate(playerPrefab, playerSpawn, Quaternion.identity);
            SetPlayer(p);
            player.GetComponent<RobotScript>().LoadRobot(chosenRobot);

            StartCoroutine(SpawnEnemies());
        }
    }

    public void SetPlayer(GameObject p)
    {
        player = p;

        player.GetComponent<PlayerScript>().moveJoystick = left;
        player.GetComponent<PlayerScript>().turnJoystick = right;

        Camera.main.GetComponent<CameraFollowScript>().SetPlayerFollow(player);
    }

    private GameObject SpawnEnemy(Robot r)
    {
        //float angle = Random.Range(0.0f, Mathf.PI * 2);
        // 15.0f * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))

        GameObject enemyObj = Instantiate(enemyPrefab, enemySpawn, Quaternion.identity);
        enemyObj.GetComponent<RobotScript>().LoadRobot(r);

        return enemyObj;
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

    IEnumerator SpawnEnemies()
    {
        float blockz = 4;
        float weaponz = 2;

        bool parity = true;

        while (true)
        {
            Robot r = null;
            //if (Random.Range(0, 1.0f) < 0.1f) r = Controller.playerRobot;
            //else r = Robot.GenerateRandomRobot((int)blockz, (int)weaponz, 0.15f);

            r = parity ? Controller.playerRobot : Robot.RandomFileRobot();
            parity = !parity;

            //r = Robot.RandomFileRobot();

            GameObject obj = SpawnEnemy(r);

            blockz += 0.5f;
            weaponz += 0.35f;

            while (obj != null && obj.transform.childCount > 0)
            {
                yield return new WaitForSeconds(10.0f);
            }
        }
    }

    public void BackPressed()
    {
        MakerScript.LoadUnsavedRobot(MakerScript.RobotName, Controller.playerRobot);
        SceneManager.LoadScene("BuildScene");
    }
    
    public void PlayerUse()
    {
        if (player != null)
            player.GetComponent<RobotScript>().Use();
    }
}
