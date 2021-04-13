using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolateController : MonoBehaviour
{
    private Vector2 robotPos;    // world position of the VISUAL robot
    private float robotRot;

    // (squared as distance measurement is squared - for speed)
    private float stopInterpolatingBoundary = 0.2f * 0.2f;
    private float rotationInterpolateBoundary = 5.0f;
    private const float interpolateConst = 0.1f; // 1.0f means instant teleportation

    private bool interpolating = false;

    private Rigidbody2D mrig;
    private void Awake()
    {
        mrig = GetComponent<Rigidbody2D>();
    }

    public Vector2 GetCenter()
    {
        if (interpolating) return robotPos;
        return mrig.worldCenterOfMass;
    }

    // Start interpolating - auto turns off when close enough
    public void Interpolate()
    {
        if (interpolating) return;

        robotPos = mrig.worldCenterOfMass;
        robotRot = mrig.rotation;

        interpolating = true;
        foreach (Interpolater i in gameObject.GetComponentsInChildren<Interpolater>())
            i.InterpolateOn();
    }

    public void StopInterpolate()
    {
        interpolating = false;
        foreach (Interpolater i in gameObject.GetComponentsInChildren<Interpolater>())
            i.InterpolateOff();
    }

    private void Update()
    {
        if (interpolating)
        {
            robotPos = Vector2.Lerp(robotPos, mrig.worldCenterOfMass, interpolateConst);
            robotRot = Mathf.Lerp(robotRot, mrig.rotation, interpolateConst); // todo: this will cause 360 degree rotations

            // Un-rotate, then re-rotate
            Matrix4x4 rotate = Matrix4x4.Rotate(Quaternion.Euler(0, 0, robotRot - mrig.rotation));

            foreach (Interpolater i in gameObject.GetComponentsInChildren<Interpolater>())
            {
                // Set rotation
                i.transform.rotation = Quaternion.Euler(0, 0, i.parent.rotation.eulerAngles.z + robotRot - mrig.rotation);

                // Get offset position
                Vector2 myoffset = (Vector2)i.parent.position - mrig.worldCenterOfMass;
                Vector2 beforeRot = robotPos + myoffset;

                // Rotate around center of mass
                i.transform.position = robotPos + (Vector2)(rotate * (beforeRot - robotPos));
            }

            if (Util.AngleDifDegree(robotRot, mrig.rotation) < rotationInterpolateBoundary &&
                Vector2.SqrMagnitude(robotPos - mrig.worldCenterOfMass) < stopInterpolatingBoundary)
            {
                StopInterpolate();
            }
        }
    }
}
