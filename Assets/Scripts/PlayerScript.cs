using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = System.Tuple<int, int>;

public class PlayerScript : MonoBehaviour
{
    // Front angle
    private float front = 0.75f;
    public RobotScript mover;
    Rigidbody2D mrig;
    Vector2 movement;
    // Start is called before the first frame update

    private bool normalMovementMode = true;

    public FixedJoystick moveJoystick, turnJoystick;

    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();

        // TODO: Better system
        if (normalMovementMode)
        {
            //turnJoystick.AxisOptions = AxisOptions.Both;
        }
        else
        {
            turnJoystick.AxisOptions = AxisOptions.Horizontal;
        }
    }

    void Update()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            movement = Vector2.zero;
            if (Input.GetKey(KeyCode.A))
                movement.x -= 1;
            if (Input.GetKey(KeyCode.D))
                movement.x += 1;
            if (Input.GetKey(KeyCode.W))
                movement.y += 1;
            if (Input.GetKey(KeyCode.S))
                movement.y -= 1;

            if (Input.GetKeyDown(KeyCode.E))
                mover.Use();
        }
    }
    void FixedUpdate()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            Vector2 worldGoal = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 comWorld = mrig.worldCenterOfMass;

            mover.Move(movement, worldGoal - comWorld);
        }
        else
        {
            // TODO: Better system
            if (normalMovementMode)
                JoystickMoveNormal();
            else
                JoystickMoveTurn();
        }
    }

    // The original system
    void JoystickMoveNormal()
    {
        if (turnJoystick.IsHeld)
        {
            mover.Move(moveJoystick.Direction, turnJoystick.Direction);
        }
        else
        {
            mover.Move(moveJoystick.Direction);
        }
    }

    // Right is turn, local
    void JoystickMoveTurn()
    {
        if (turnJoystick.IsHeld)
        {
            float angleChange = -Mathf.PI * turnJoystick.Horizontal * 0.9f;
            float cAng = mrig.rotation * Mathf.Deg2Rad - front * Mathf.PI * 2;
            Vector2 x = new Vector2(Mathf.Cos(cAng + angleChange), Mathf.Sin(cAng + angleChange));
            mover.Move(moveJoystick.Direction, x);
        }
        else
        {
            mover.Move(moveJoystick.Direction);
        }
    }
}


/*
 * 
        float ang = Vector2.SignedAngle(new Vector2(1, 0), worldGoal - comWorld) / 360.0f;
        if (ang < 0) ang += 1;
        float curAng = transform.rotation.eulerAngles.z / 360.0f - front;
        if (curAng < 0) curAng += 1;

        // get the two cases for rotation
        float a, b;
        if (curAng < ang)
        {
            a = ang - curAng;
            b = ang - 1 - curAng;
        }
        else
        {
            a = ang + 1 - curAng;
            b = ang - curAng;
        }
        float turn;
        if (Mathf.Abs(a) < Mathf.Abs(b)) turn = a;
        else turn = b;
       */