using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterScript : StaticBlock, IUsableBlock
{
    [SerializeField]
    private float force;

    [SerializeField]
    private float OnTime;
    public override BlockType Type => BlockType.THRUSTER;

    private bool isThrusting = false;

    private Animator anim;

    private Rigidbody2D mrig;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        mrig = transform.parent.GetComponent<Rigidbody2D>();
    }
    public void Use()
    {
        StartCoroutine(WaitUse());
    }

    IEnumerator WaitUse()
    {
        isThrusting = true;
        anim.SetBool("isThrusting", isThrusting);
        yield return new WaitForSeconds(OnTime);
        isThrusting = false;
        anim.SetBool("isThrusting", isThrusting);
    }

    private void FixedUpdate()
    {
        if (isThrusting)
        {
            mrig.AddForceAtPosition(transform.up * force, transform.position);
        }
    }


}
