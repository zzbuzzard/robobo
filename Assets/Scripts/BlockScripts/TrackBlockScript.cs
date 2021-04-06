using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBlockScript : MovementBlock
{
    public override BlockType Type => BlockType.TRACK;
    public override WheelType Wheel => WheelType.TRACK;

    private Animator anim;
    private Rigidbody2D mrig;

    [SerializeField]
    private float forward_friction;

    [SerializeField]
    private float lateral_friction;


    private RobotScript controller;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        mrig = transform.parent.GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controller = transform.parent.GetComponent<RobotScript>();
    }

    void FixedUpdate()
    {
        if (parent == null || GetParent().currentWheelType != WheelType.TRACK) return;


        float mass = mrig.mass;
        Vector2 vel = mrig.GetPointVelocity(transform.position);
        int num_tracks = controller.NumWheels(WheelType.TRACK);
        float speed = Vector2.Dot(transform.right, vel);
        float across = Vector2.Dot(transform.up, vel);
        mrig.AddForceAtPosition(- transform.right * forward_friction * mass * speed/num_tracks, transform.position);
        mrig.AddForceAtPosition(- transform.up * lateral_friction * mass * across/num_tracks, transform.position);
        //Debug.Log(transform.forward);
        //Debug.Log(vel);
        anim.SetFloat("Speed", speed * 0.25f);
    }

    protected override void HandleDeath()
    {
        base.HandleDeath();
        anim.SetFloat("Speed", 0.0f);
    }
}
