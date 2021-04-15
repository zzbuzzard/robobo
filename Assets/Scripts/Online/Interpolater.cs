using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolater : MonoBehaviour
{
#if UNITY_SERVER
    private void Awake()
    {
        Destroy(gameObject);
    }
#else
    public Transform parent;
    Block parentBlock;
    public bool isInterpolating;

    public bool initialised;

    private void Awake()
    {
        parentBlock = GetComponentInParent<Block>();
    }

    public void InterpolateOn()
    {
        isInterpolating = true;
    }

    public void InterpolateOff()
    {
        isInterpolating = false;
    }

    private void Update()
    {
        if (transform.parent != null && initialised)
            transform.parent = null;

        if (parentBlock == null)
        {
            transform.parent = parent;
            transform.localPosition = Vector2.zero;
            transform.localRotation = Quaternion.identity;

            Destroy(this);
            return;
        }

        if (!isInterpolating)
        {
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
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
#endif
}
