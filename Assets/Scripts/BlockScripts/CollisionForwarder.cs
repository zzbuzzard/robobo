using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionForwardParent
{
    void ChildCollision(Collision2D collision);
    void ChildCollisionStay(Collision2D collision);
}

public class CollisionForwarder : MonoBehaviour
{
    public Block parent;

    //protected PistonScript parentPiston;
    //public GameObject parentObject;

    //void Start()
    //{
    //    parentPiston = parentObject.GetComponent<PistonScript>();
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ((ICollisionForwardParent)parent).ChildCollision(collision);
        //parentPiston.PistonHeadCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ((ICollisionForwardParent)parent).ChildCollisionStay(collision);
    }
}
