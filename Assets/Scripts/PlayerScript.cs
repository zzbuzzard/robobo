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
    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();
    }

    void Update()
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
    void FixedUpdate()
    {
        int c = transform.childCount;
        if (c == 0)
        {
            Destroy(gameObject);
            return;
        }
        /////////////////////// Move:
        Vector2 worldGoal = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 comWorld = mrig.worldCenterOfMass;

        mover.Move(movement, worldGoal - comWorld);
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