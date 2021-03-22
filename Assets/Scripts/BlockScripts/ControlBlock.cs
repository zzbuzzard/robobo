using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Should inherit from some kinda StaticBlock class
public class ControlBlock : Block
{
    public override bool IsControl()
    {
        return true;
    }
}