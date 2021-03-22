using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonScript : Block
{
    public Transform bone;

    void Start()
    {
        bone = transform.Find("Piston");
    }
}
