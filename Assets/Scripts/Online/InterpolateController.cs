using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Maths: If P(t) = v*t
//        And U(t) = Lerp(U(t-1), P(t), x)
//       Then U(t) = P(t) - ((1-x)/x)*v
//       So we end up (1-x)/x * v behind, so we should predict that far ahead

public class InterpolateController : MonoBehaviour
{
#if UNITY_SERVER
#else
    private Vector2 robotPos;    // world position of the VISUAL robot
    private float robotRot;

    // Interpolation stuff: last frame
    public bool isLocal = false;
    public Vector2 pastPos;
    public Vector2 pastVel;

    // (squared as distance measurement is squared - for speed)
    private float stopInterpolatingBoundary = 0.1f * 0.1f;
    private float rotationInterpolateBoundary = 3.0f;

    private const float timeToReachTarget = 0.1f;
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
            i.initialised = true;
        }
    }

    public Vector2 GetCenter()
    {
        if (interpolating) return robotPos;
        return mrig.worldCenterOfMass;
    }

    // Start interpolating - auto turns off when close enough
    public void StartInterpolate()
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

    // Interpolates 1 frame
    // Should be called BETWEEN FORCE APPLICATION and PHYSICS SIMULATION for perfect results.
    private void FixedUpdate()
    {
        if (interpolating)
        {
            
            //                 VELOCITY APPROACH

            Vector2 realVel;

            // To be honest, I don't really know what's going on here anymore
            // The general idea though, is we move towards where the player will be in X seconds
            //  and try to get there in X seconds.
            if (Controller.isLocalGame)
            {
                realVel = (mrig.worldCenterOfMass
                    + timeToReachTarget * mrig.velocity - robotPos)
                    / timeToReachTarget;
            }
            else
            {
                if (isLocal)
                {
                    realVel = (pastPos
                        + timeToReachTarget * pastVel - robotPos)
                        / timeToReachTarget;
                }
                else
                {
                    realVel = (mrig.worldCenterOfMass
                        + timeToReachTarget * mrig.velocity - robotPos)
                        / timeToReachTarget;
                }
            }


            vel = Vector2.Lerp(vel, realVel, velocityInterpolate);

            float realAngularVel = (mrig.rotation + timeToReachTarget * mrig.angularVelocity - robotRot) / timeToReachTarget;
            angularVel = Mathf.Lerp(angularVel, realAngularVel, velocityInterpolate);

            robotPos = robotPos + vel * Time.fixedDeltaTime;
            robotRot = robotRot + angularVel * Time.fixedDeltaTime;
            

            /*
                         //DIRECT LERP - BUT PERFECTLY MOVES TO VELOCITY POS

            const float x = 0.3f;
            const float localMul = 1 / x;
            //const float onlineMul = (1 - x) / x;

            if (Controller.isLocalGame)
                robotPos = Vector2.Lerp(robotPos, mrig.worldCenterOfMass + mrig.velocity * Time.fixedDeltaTime * localMul, x); // interpolateConst);
            else
                robotPos = Vector2.Lerp(robotPos, pastPos + pastVel * Time.fixedDeltaTime * localMul, x); // interpolateConst);

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

            // TODO: Should we ever stop interpolating?
            //if (Util.AngleDifDegree(robotRot, mrig.rotation) < rotationInterpolateBoundary &&
            //    Util.SqDist(robotPos, mrig.worldCenterOfMass) < stopInterpolatingBoundary)
            //{
            //    StopInterpolate();
            //}
        }
    }
#endif
}
