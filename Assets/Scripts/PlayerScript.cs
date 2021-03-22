using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XY = System.Tuple<int, int>;

public class PlayerScript : MonoBehaviour
{
    // Front angle
    private float front = 0.75f;
    public MovementScript mover;
    Rigidbody2D mrig;

    // Start is called before the first frame update
    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();


        // Example robot load:
        //IDictionary<XY, Robot.BlockType> dict = new Dictionary<XY, Robot.BlockType>();

        //dict[new XY(0, 0)] = Robot.BlockType.CONTROL;
        //dict[new XY(1, 0)] = Robot.BlockType.METAL;
        //dict[new XY(-1, 0)] = Robot.BlockType.METAL;
        //dict[new XY(0, 1)] = Robot.BlockType.SPIKE;

        //List<Vector2> wheelz = new List<Vector2>();
        //wheelz.Add(new Vector2(-1.5f, 0.0f));
        //wheelz.Add(new Vector2(11.5f, 0.0f));

        //Robot test = new Robot(dict, new XY(0, 0), wheelz);
        //mover.LoadRobot(test);
    }

    void FixedUpdate()
    {
        /////////////////////// Move:
        Vector2 movement = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            movement.x -= 1;
        if (Input.GetKey(KeyCode.D))
            movement.x += 1;
        if (Input.GetKey(KeyCode.W))
            movement.y += 1;
        if (Input.GetKey(KeyCode.S))
            movement.y -= 1;
        
        movement = transform.InverseTransformDirection(movement);
        float cancelMoment = mover.ApplyMovement(movement);

        /////////////////////// Rotate:
        Vector2 worldGoal = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 comWorld = mrig.worldCenterOfMass;

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

        turn = mover.CalculateTorque(turn);

        mover.ApplyTorque(turn - cancelMoment);
    }
}
