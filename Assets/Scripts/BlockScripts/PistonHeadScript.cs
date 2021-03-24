using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonHeadScript : MonoBehaviour
{
    protected PistonScript parentPiston;
    public GameObject parentObject;

    void Start()
    {
        parentPiston = parentObject.GetComponent<PistonScript>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        parentPiston.PistonHeadCollision(collision);

    }
}
