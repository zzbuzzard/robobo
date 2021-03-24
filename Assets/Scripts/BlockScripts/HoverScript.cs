using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverScript : MovementBlock
{
    public override WheelType GetWheelType()
    {
        return WheelType.HOVER;
    }
}
