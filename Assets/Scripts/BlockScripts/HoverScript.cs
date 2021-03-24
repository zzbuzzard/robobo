using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverScript : MovementBlock
{
    public override BlockType Type => BlockType.HOVER;
    public override WheelType Wheel => WheelType.HOVER;
}
