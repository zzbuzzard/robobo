using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonScript : Block
{
    public Transform bone;

    protected override void Start()
    {
        // Need to call Block.Start
        base.Start();
        bone = transform.Find("Piston");
    }
}
