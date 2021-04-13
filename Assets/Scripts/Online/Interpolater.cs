using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Interpolate stuff
public class Interpolater : MonoBehaviour
{
    public Transform parent;
    //private bool isInterpolating;

    public void InterpolateOn()
    {

    }

    public void InterpolateOff()
    {
        transform.localPosition = Vector2.zero;
        transform.localRotation = Quaternion.identity;
    }

    // TODO: Try update. I think this would lead to camera jitter, though
    //void FixedUpdate()
    //{
        //if (isInterpolating)
        //{
        //    myPos = Vector2.Lerp(myPos, parent.position, 0.1f);
        //    myRot = Quaternion.Lerp(myRot, parent.rotation, 0.1f);

        //    transform.position = myPos;
        //    transform.rotation = myRot;

        //    // TODO: Check rotation isnt too far off too
        //    if (Vector2.SqrMagnitude(parent.position - transform.position) < stopInterpolatingBoundary)
        //    {
        //        isInterpolating = false;
        //    }
        //}
        //else
        //{
        //    transform.position = parent.position;
        //    transform.rotation = parent.rotation;
        //}
    //}
}
