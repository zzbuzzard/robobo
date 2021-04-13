using Mirror;
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
    private void Start()
    {
        if (!isOnline)
        {
            // Spawn player
            Robot chosenRobot = Controller.playerRobot;
            GameObject p = Instantiate(playerPrefab, playerSpawn, Quaternion.identity);
            p.GetComponent<RobotScript>().enabled = true;
            p.GetComponent<PlayerScript>().enabled = true;
            Destroy(p.GetComponent<PlayerOnline>());
            SetPlayer(p);
            NetworkServer.Spawn(p);
            player.GetComponent<RobotScript>().LoadRobot(Controller.playerRobot);

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
        NetworkServer.Spawn(enemyObj);
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
            yield return new WaitForSeconds(2.0f);

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
        if (isOnline)
        {
            NetMan m = GameObject.Find("NetworkManager").GetComponent<NetMan>();
            m.StopServer();
            m.StopClient();
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            NetworkServer.DisconnectAll();
            NetworkClient.Disconnect();
            MakerScript.LoadUnsavedRobot(MakerScript.RobotName, Controller.playerRobot);
            SceneManager.LoadScene("BuildScene");
        }
    }
    
    public void PlayerUse()
    {
        if (player != null)
            player.GetComponent<PlayerScript>().useNextFrame = true;
    }
}
