using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneScript : MonoBehaviour
{

    public void Awake()
    {
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
#if UNITY_SERVER
        SceneManager.LoadScene("OnlineScene");
#endif       
    }


    public void RobotsClicked()
    {
        SceneManager.LoadScene("SelectRobot");
    }

    public void OnlineClicked()
    {
        Controller.isLocalGame = false;
        SceneManager.LoadScene("OnlineScene");
    }
}
