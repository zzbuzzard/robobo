using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeScript : MonoBehaviour
{
    public GameObject sparks;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    float force = 5000.0f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.rigidbody == null) return;
        if (collision.transform.parent == null || collision.transform.parent == transform.parent) return;

        print("kachow");

        Vector2 avg_pos = Vector2.zero;
        Vector2 avg_normal = Vector2.zero;

        for (int i=0; i<collision.contactCount; i++)
        {
            avg_normal += collision.GetContact(i).normal;
            avg_pos += collision.GetContact(i).point;
        }

        avg_pos /= collision.contactCount;
        avg_normal /= collision.contactCount;

        collision.rigidbody.AddForceAtPosition(-avg_normal.normalized * force, avg_pos);

        Instantiate(sparks, (Vector3)avg_pos + new Vector3(0, 0, -1), Quaternion.identity);
    }
}
