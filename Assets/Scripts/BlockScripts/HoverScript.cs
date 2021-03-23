using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverScript : Block
{

    public GameObject parentObject;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        registerParent();

    }

    void registerParent()
    {
        parent.AddWheel(parentObject);
    }
    public override wheelType WheelType()
    {
        return wheelType.HOVER;
    }
    // Update is called once per frame
    void Update()
    {

    }
    public override void TakeDamage(int damage)
    {
        Debug.Log("WEEEEEEEEEEEEE I'M A BLOCK");
        base.TakeDamage(damage);
        
    }
}
