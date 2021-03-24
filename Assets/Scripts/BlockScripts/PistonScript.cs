using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonScript : UsableWeaponBlock
{
    public float damage = 0;
    public Transform bone;
    public float force = 10000f;
    private Animation anim;
    private bool enabled;
    protected override void Start()
    {
        // Need to call Block.Start
        base.Start();
        bone = transform.Find("Piston");
        anim = GetComponentInChildren<Animation>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Entered");
    }
    public override void Use()
    {
        enabled = true;
        anim.Play();
        enabled = false;
    }
    public void PistonHeadCollision(Collision2D collision)
    {
        if (!anim.isPlaying) return;
        Block b = collision.collider.transform.GetComponent<Block>();


        // Don't do sparks if it's nobody or if it's part of the same parent
        if (b == null) return;

        // Current method: Apply force at avg contact position, opposite to avg normal
        //  It's ok, but could do with a variable force using e.g. Contact.normalImpulse or whatever
        Vector2 avg_pos = Vector2.zero;
        Vector2 avg_normal = Vector2.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            avg_normal += collision.GetContact(i).normal;
            avg_pos += collision.GetContact(i).point;
        }
        avg_pos /= collision.contactCount;
        avg_normal /= collision.contactCount;

        collision.rigidbody.AddForceAtPosition(-avg_normal.normalized * force, avg_pos);
    }
    public override void DealDamage(Block target, float damage)
    {

    }
}

