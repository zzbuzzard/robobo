using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneScript : MonoBehaviour
{

#if UNITY_SERVER
    public void Awake()
    {
        SceneManager.LoadScene("OnlineScene");
    }
#endif
    public void RobotsClicked()
    {
        SceneManager.LoadScene("SelectRobot");
    }

    public void OnlineClicked()
    {
        SceneManager.LoadScene("OnlineScene");
    }
}
