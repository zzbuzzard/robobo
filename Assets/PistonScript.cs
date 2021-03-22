using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonScript : MonoBehaviour
{
    public Transform bone;
    // Start is called before the first frame update
    void Start()
    {
        bone = GetComponent<Transform>().Find("Piston");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (bone != null)
            {
                bone.localPosition += new Vector3(0,1,0);
            }
            else
            {
                Debug.Log("bone is null!");
            }
        }
    }
}
