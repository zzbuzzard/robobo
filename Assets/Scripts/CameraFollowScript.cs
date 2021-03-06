using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    private static Vector3 offset = new Vector3(0, 0, -10.0f);
    private const float mul = 0.3f;
    private static float induced_mul;
    private RobotScript follow;

    public void SetPlayerFollow(GameObject obj)
    {
        induced_mul = Mathf.Pow(mul, Time.fixedDeltaTime);
        follow = obj.GetComponent<RobotScript>();
    }

    private void FixedUpdate()
    {
        if (follow == null) return;

        Vector2 goal = follow.GetControlPos();

        // dist = mul^t
        // dist(0) = dist goal and pos
        // dist(dt) = dist(0) * mul^dt
        
        transform.position = (Vector3)Vector2.Lerp(transform.position, goal, induced_mul) + offset;
    }
}
