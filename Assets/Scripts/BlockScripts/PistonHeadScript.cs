using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonHeadScript : Block
{


    protected PistonScript parent;
    public GameObject parentObject;

    void Start()
    {
        parent = parentObject.GetComponent<PistonScript>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        parent.PistonHeadCollision(collision);

    }

    public override void TakeDamage(int damage)
    {
        parent.TakeDamage(damage);
    }
}
