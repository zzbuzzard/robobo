using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectRobotController : MonoBehaviour
{
    List<string> names;

    private void Start()
    {
        IDictionary<string, Robot> robots = Robot.LoadAllRobots();

        names = new List<string>();
        foreach (string s in robots.Keys)
        {
            names.Add(s);
        }
    }

    private void CreateRobotObj(Robot r, int id)
    {
    }

    public void RobotClicked(int id)
    {

    }
}
