using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragCamera : MonoBehaviour
{
    float dragFrom;
    float myPosAtDrag;
    float prevPos;
    int draggingFinger = -1;

    bool dragging = false;
    float vel = 0;

    private float dragStartTime;
    private float elapsedTime = 0;

    private float dragDist = 0;

    private void StartDrag(float pos, int fingerID)
    {
        dragging = true;

        draggingFinger = fingerID;
        dragFrom = pos;

        myPosAtDrag = transform.position.x;
        prevPos = dragFrom;
        dragStartTime = elapsedTime;
        dragDist = 0;
    }

    void Start()
    {
        
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        List<Touch> touches = new List<Touch>(Input.touches);

        // Scuffed:
        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            if (Input.GetMouseButton(0))
            {
                Touch t = new Touch();
                t.fingerId = 0;
                t.position = Input.mousePosition;
                
                if (!dragging) t.phase = TouchPhase.Began;
                else t.phase = TouchPhase.Moved;

                touches.Add(t);
            }
        }

        // Find new starting finger, if any
        foreach (Touch t in touches)
        {
            if (t.phase == TouchPhase.Began)
            {
                StartDrag(t.position.x, t.fingerId);
                break;
            }
        }

        // Find continuation of current drag
        Touch myT = default;
        bool found = false;
        foreach (Touch t in touches)
        {
            if (t.fingerId == draggingFinger)
            {
                myT = t;
                found = true;

                break;
            }
        }

        if (found)
        {
            float newX = myT.position.x;

            // TODO: This is scuffed. For some reason, about 80% of the time, prevPos == newX
            // (so speed is kinda random)
            float v = ScreenToWorldScale(prevPos - newX) / Time.deltaTime;
            vel = vel / 2 + v/2;
            dragDist += Mathf.Abs(prevPos - newX);

            float off = ScreenToWorldScale(dragFrom - newX);

            transform.position = new Vector3(myPosAtDrag + off, 0, -10.0f);

            prevPos = newX;
        }
        else
        {
            dragging = false;
            draggingFinger = -1;

            transform.position += new Vector3(vel * Time.deltaTime, 0);
        }
    }

    private void FixedUpdate()
    {
        vel *= 0.85f;
    }

    // TODO: There's defo a better way of doing this
    private float ScreenToWorldScale(float screenDist)
    {
        return  Camera.main.ScreenToWorldPoint(new Vector2(screenDist, 0)).x
            - Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).x;
    }

    public float LastDragDist()
    {
        return ScreenToWorldScale(dragDist);
    }
}
