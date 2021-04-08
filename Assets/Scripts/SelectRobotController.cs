using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using XY = UnityEngine.Vector2Int;

public class SelectRobotController : MonoBehaviour
{
    public GameObject parentPrefab;
    public GameObject blockPrefab;
    public GameObject createButton;
    public DragCamera dragCamera;

    private float spacing = 15.0f;
    List<string> names;

    int newName = 0;

    private void Start()
    {
        // Load and draw robots
        IDictionary<string, Robot> robots = Robot.LoadAllRobots();
        names = new List<string>();
        int id = 0;
        foreach (string s in robots.Keys)
        {
            names.Add(s);
            CreateRobotObj(robots[s], id);
            id++;
        }

        newName = id;
        createButton.GetComponent<ClickScript>().onClick = CreateNew;
        createButton.transform.position = new Vector2(spacing * id, 0);
    }

    private void CreateRobotObj(Robot r, int id)
    {
        GameObject parent = Instantiate(parentPrefab, Vector2.zero, Quaternion.identity);
        parent.GetComponent<ClickScript>().onClick = delegate() { RobotClicked(id); };

        // Get average block position so that we can center it
        Vector2 avgLocalPos = Vector2.zero;
        int count = 0;
        foreach (XY xy in r.blockTypes.Keys)
        {
            avgLocalPos += (Vector2)xy;
            count++;
        }
        avgLocalPos /= count;

        Bounds bounds = new Bounds(new Vector2(0, 0), new Vector2(0, 0));

        foreach (XY xy in r.blockTypes.Keys)
        {
            BlockType type = r.blockTypes[xy];
            int rot = r.rotations[xy];

            Quaternion rotation = Quaternion.Euler(0, 0, rot * 90.0f);

            GameObject block = Instantiate(blockPrefab, parent.transform, false);
            block.transform.localPosition = (Vector2)xy - avgLocalPos;
            block.transform.localRotation = rotation;

            block.GetComponent<SpriteRenderer>().sprite = BlockInfo.blockInfos[(int)type].showSprite;

            bounds.Encapsulate(block.GetComponent<Renderer>().bounds);
        }

        // center + offset = realCenter
        // center = realCenter - offset
        parent.transform.position = new Vector2(id * spacing, 0);

        parent.GetComponent<BoxCollider2D>().size = bounds.size * 1.1f;
        parent.GetComponent<BoxCollider2D>().offset = bounds.center;
    }

    private const float dragBound = 1.0f;
    public void RobotClicked(int id)
    {
        if (dragCamera.LastDragDist() >= dragBound) return;

        PlayerPrefs.SetString("PREF_ROBOT", names[id]);
        Controller.playerRobot = Robot.LoadRobotFromName(names[id]);

        MakerScript.LoadSavedRobot(names[id]);
        SceneManager.LoadScene("BuildScene");
    }

    public void CreateNew()
    {
        if (dragCamera.LastDragDist() >= dragBound) return;

        MakerScript.LoadSavedRobot("robot" + newName);
        SceneManager.LoadScene("BuildScene");
    }

    public void BackClicked()
    {
        SceneManager.LoadScene("MainScene");
    }
}
