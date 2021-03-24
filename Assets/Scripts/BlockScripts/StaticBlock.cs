using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaticBlock : Block
{
    public override WheelType Wheel => WheelType.NONE;
}
