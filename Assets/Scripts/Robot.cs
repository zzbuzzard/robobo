using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class to store a robot (it's blocks, stats, etc...)
public class Robot
{
    public enum BlockType
    {
        CONTROL,
        METAL,
        SPIKE
    }
    
    // relative to Resources/Prefabs/BlockPrefabs
    public static string[] blockTypePaths = { };
    public static GameObject[] blockTypePrefabs;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
