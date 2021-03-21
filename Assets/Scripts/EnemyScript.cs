using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private float front = 0.75f;
    public MovementScript mover;
    Rigidbody2D mrig;

    PlayerScript player;

    // Start is called before the first frame update
    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerScript>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 playerPos = player.transform.TransformPoint(player.GetComponent<Rigidbody2D>().centerOfMass);
        Vector2 mPos = mrig.worldCenterOfMass;

        print(playerPos);

        /////////////////////// Move:
        Vector2 movement = (playerPos - mPos).normalized;
        
        movement = transform.InverseTransformDirection(movement);
        float cancelMoment = mover.ApplyMovement(movement);

        /////////////////////// Rotate:
        float ang = Vector2.SignedAngle(new Vector2(1, 0), playerPos - mPos) / 360.0f;
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
