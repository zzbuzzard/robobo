using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneScript : MonoBehaviour
{
    public void RobotsClicked()
    {
        SceneManager.LoadScene("SelectRobot");
    }

    public void OnlineClicked()
    {
        SceneManager.LoadScene("OnlineScene");
    }
}
