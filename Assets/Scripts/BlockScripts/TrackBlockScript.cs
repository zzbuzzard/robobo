using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBlockScript : Block
{
    public override BlockType Type => BlockType.HOVER;
    public override WheelType Wheel => WheelType.HOVER;

    private Animator anim;
    private Rigidbody2D mrig;
    // Start is called before the first frame update
    void Start()
    {
        mrig = transform.parent.GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 vel = mrig.GetPointVelocity(transform.position);
        float speed = Vector2.Dot(transform.forward, vel);
        anim.SetFloat("speed", speed);
    }
}
