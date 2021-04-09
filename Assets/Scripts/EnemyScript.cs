using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private float front = 0.75f;
    public RobotScript mover;
    Rigidbody2D mrig;

    PlayerScript player;

    // Start is called before the first frame update
    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerScript>();
    }

    float useTimer = 0.0f;

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 playerPos = player.mover.GetControlPos();
        Vector2 mPos = mrig.worldCenterOfMass;

        mover.Move((playerPos - mPos).normalized, playerPos - mPos);

        useTimer -= Time.deltaTime;
        if (Vector2.Distance(playerPos, mPos) < 20.0f)
        {
            if (useTimer <= 0.0f)
            {
                // TODO: Should really be server only? I guess?
                mover.LocalUse();
                useTimer = 1.0f;
            }
        }
    }
}
