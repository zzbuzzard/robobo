using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Should inherit from some kinda StaticBlock class
public class ControlBlock : StaticBlock
{
    public override BlockType Type => BlockType.CONTROL;
}