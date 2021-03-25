using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBlockScript : Block
{
    public override BlockType Type => BlockType.HOVER;
    public override WheelType Wheel => WheelType.TRACK;

    private Animator anim;
    private Rigidbody2D mrig;
    public float forward_friction;
    public float lateral_friction;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        mrig = transform.parent.GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mrig == null) {
            mrig = GetComponent<Rigidbody2D>();
            return;
                }
        float mass = mrig.mass;
        Vector2 vel = mrig.GetPointVelocity(transform.position);
        
        float speed = Vector2.Dot(transform.right, vel);
        float across = Vector2.Dot(transform.up, vel);
        mrig.AddForceAtPosition(- transform.right * forward_friction * mass * speed, transform.position);
        mrig.AddForceAtPosition(- transform.up * lateral_friction * mass * across, transform.position);
        //Debug.Log(transform.forward);
        //Debug.Log(vel);
        anim.SetFloat("Speed", speed * 0.25f);
    }
}
