using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = System.Tuple<int, int>;

public class PlayerScript : MonoBehaviour
{
    Vector2 movement;
    public bool useNextFrame = false;

    // Front angle
    private float front = 0.75f;
    public RobotScript mover;
    public Rigidbody2D mrig;
    
    private bool normalMovementMode = true;

    public FixedJoystick moveJoystick, turnJoystick;

    void Start()
    {
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
                useNextFrame = true;
        }
    }

    void FixedUpdate()
    {
#if UNITY_SERVER
#else
        if (Controller.isLocalGame)
        {
            mover.Move(GetMove(), GetTurn());
            if (useNextFrame)
            {
                useNextFrame = false;
                mover.Use();
            }
        }
#endif

        useNextFrame = false;
    }

    public Vector2 GetMove()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            return movement;
        }

        return moveJoystick.Direction;
    }

    public Vector2 GetTurn()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            Vector2 worldGoal = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 comWorld = mrig.worldCenterOfMass;

            return worldGoal - comWorld;
        }

        if (!turnJoystick.IsHeld) return Vector2.zero;

        if (normalMovementMode)
        {
            return turnJoystick.Direction;
        }
        else
        {
            float angleChange = -Mathf.PI * turnJoystick.Horizontal * 0.9f;
            float cAng = mrig.rotation * Mathf.Deg2Rad - front * Mathf.PI * 2;
            return new Vector2(Mathf.Cos(cAng + angleChange), Mathf.Sin(cAng + angleChange));
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