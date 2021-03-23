using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverScript : MovementBlock
{
    public GameObject parentObject;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        RegisterParent();
    }

    void RegisterParent()
    {
        parent.AddWheel(parentObject);
    }

    public override WheelType GetWheelType()
    {
        return WheelType.HOVER;
    }
}
