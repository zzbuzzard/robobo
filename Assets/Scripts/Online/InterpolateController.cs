using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Maths: If P(t) = v*t
//        And U(t) = Lerp(U(t-1), P(t), x)
//       Then U(t) = P(t) - ((1-x)/x)*v
//       So we end up (1-x)/x * v behind, so we should predict that far ahead

public class InterpolateController : MonoBehaviour
{
    private Vector2 robotPos;    // world position of the VISUAL robot
    private float robotRot;

    // (squared as distance measurement is squared - for speed)
    private float stopInterpolatingBoundary = 0.1f * 0.1f;
    private float rotationInterpolateBoundary = 3.0f;
    //private const float interpolateConst = 0.25f; // 1.0f means instant teleportation

    //private const int numFramesToReachTarget = 5;
    private const float timeToReachTarget = 0.15f;
    private const float velocityInterpolate = 0.6f;
    private Vector2 vel;
    private float angularVel;

    private bool interpolating = false;

    public Rigidbody2D mrig;

    private Interpolater[] interpolaters;

    private bool initialised = false;
    public void Initialise()
    {
        initialised = true;
        interpolaters = GetComponentsInChildren<Interpolater>();
        foreach (Interpolater i in interpolaters)
        {
            i.transform.parent = null;
        }
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
        foreach (Interpolater i in interpolaters)
            i.InterpolateOn();
    }

    public void StopInterpolate()
    {
        interpolating = false;
        foreach (Interpolater i in interpolaters)
            i.InterpolateOff();
    }

    //TODO: Delete
    private bool specialMode = true;

    private void FixedUpdate()
    {
        if (OnlineGameControl.isResimulating)
        {
            Debug.Log("Resimulating wtf");
        }
        if (interpolating)
        {

            //                 VELOCITY APPROACH
            // Speed * number of frames

            // Speed * frame * framesToReachTarget
            // framesToReachTarget = timeWait / Time.fixedDeltaTime

            Vector2 realVel = (mrig.worldCenterOfMass + timeToReachTarget * mrig.velocity - robotPos) / timeToReachTarget;
            vel = Vector2.Lerp(vel, realVel, velocityInterpolate);

            float realAngularVel = (mrig.rotation + timeToReachTarget * mrig.angularVelocity - robotRot) / timeToReachTarget;
            angularVel = Mathf.Lerp(angularVel, realAngularVel, velocityInterpolate);

            robotPos = robotPos + realVel * Time.fixedDeltaTime;
            robotRot = robotRot + angularVel * Time.fixedDeltaTime;


            /*
                         DIRECT LERP - BUT PERFECTLY MOVES TO VELOCITY POS

            const float x = 0.3f;
            const float mull = 1 / x; // (1 - x) / x;

            if (Input.GetKeyDown(KeyCode.H))
            {
                specialMode = !specialMode;
                Debug.Log("SPECIAL MODE: " + specialMode);
            }

            if (specialMode)
            {
                robotPos = Vector2.Lerp(robotPos, mrig.worldCenterOfMass + mrig.velocity * Time.fixedDeltaTime * mull, x); // interpolateConst);
            }
            else
            {
                robotPos = Vector2.Lerp(robotPos, mrig.worldCenterOfMass, x); // interpolateConst);
            }
            robotRot = Mathf.Lerp(robotRot, mrig.rotation, x); // rotationInterpolate); // todo: this will cause 360 degree rotations
             */

            // Un-rotate, then re-rotate
            Matrix4x4 rotate = Matrix4x4.Rotate(Quaternion.Euler(0, 0, robotRot - mrig.rotation));

            if (initialised)
            {
                foreach (Interpolater i in interpolaters)
                {
                    if (i == null || i.parent == null) continue;

                    // Set rotation
                    i.transform.rotation = Quaternion.Euler(0, 0, i.parent.rotation.eulerAngles.z + robotRot - mrig.rotation);

                    // Get offset position
                    Vector2 myoffset = (Vector2)i.parent.position - mrig.worldCenterOfMass;
                    Vector2 beforeRot = robotPos + myoffset;

                    // Rotate around center of mass
                    i.transform.position = robotPos + (Vector2)(rotate * (beforeRot - robotPos));
                }
            }

            if (Util.AngleDifDegree(robotRot, mrig.rotation) < rotationInterpolateBoundary &&
                Vector2.SqrMagnitude(robotPos - mrig.worldCenterOfMass) < stopInterpolatingBoundary)
            {
                //StopInterpolate();
            }
        }
    }
}
