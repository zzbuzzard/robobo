using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// A script class, but it's abstract so can never be used
// Its purpose is to define base behaviour for all blocks

// Enforce blocks have a collider
[RequireComponent(typeof(Collider2D))]
public abstract class Block : NetworkBehaviour
{
    // Things that change at runtime:
    [SerializeField]
    private float hp;
    public int x, y;
    private bool dead = false;
    protected GameObject parent; // the RobotScript of my parent

    private bool initialisedByServer = false;

    // Things that are fixed:
    public abstract BlockType Type { get; }
    public abstract WheelType Wheel { get; }

    [SerializeField]
    private float density;

    protected Collider2D myCollider; // base class for box collider, polygon collider, etc.
    private FlashScript flashScript;

    private float maxHP;

    public void ServerInit(GameObject parentObj, int x, int y)
    {
        if (!isServer)
        {
            Debug.LogWarning("Block.ServerInit called by a client!");
            return;
        }
        Debug.Log("Server calling init on block");

        parent = parentObj;
        transform.SetParent(parentObj.transform);
        this.x = x;
        this.y = y;
        initialisedByServer = true;

        UpdateClients(parentObj, x, y);
    }

    [ClientRpc]
    void UpdateClients(GameObject parentObj, int x, int y)
    {
        parent = parentObj;
        transform.SetParent(parentObj.transform);
        this.x = x;
        this.y = y;
        initialisedByServer = true;
    }

    public bool IsInitialisedByServer()
    {
        return initialisedByServer;
    }

    protected virtual void Start()
    {
        maxHP = hp;
        myCollider = GetComponent<Collider2D>();
        flashScript = GetComponent<FlashScript>();
        myCollider.density = density;
    }

    // I have no idea why this is necessary. C# sucks
    // If you are a SpikeScript, and you have a Block b, then you can't access b.parent even though it's protected...
    public RobotScript GetParent()
    {
        return parent.GetComponent<RobotScript>();
    }

    // Damage should be done generally through Damageable.
    public virtual void TakeDamage(float damage)
    {
        hp -= damage;
        ChangeHpDisplay();
        if (hp <= 0)
        {
            Die();
        }
        
    }

    // TODO: Show cracks etc
    private void ChangeHpDisplay()
    {
        flashScript.Flash();
    }

    public bool IsDead() { return dead; }

    // SOMEONE ELSES' HP HIT ZERO: We were told by parent
    public void Detach()
    {
        // make sure everything only dies once
        if (dead) return;
        dead = true;

        // detach, but don't tell parent as we came from the parent
        HandleDeath();
        Destroy(this); // destroy this component
    }

    // TODO: Some kinda particle effect?
    // HP HIT ZERO: Tell parent
    private void Die()
    {
        // make sure everything only dies once
        if (dead) return;
        dead = true;

        // detach and tell parent
        HandleDeath();
        GetParent().RemoveBlock(this); // Must be last, as this statement may delete this object
        Destroy(this); // destroy this component
    }

    // It's hard sometimes, I know
    protected virtual void HandleDeath()
    {
        //ResetMaterial();

        Rigidbody2D rig = gameObject.AddComponent<Rigidbody2D>();
        rig.gravityScale = 0.0f;
        rig.drag = 5;
        rig.angularDrag = 1;
        rig.mass = density * 1.5f * 1.5f; // should be collider area, but that doesn't seem to be gettable
        rig.velocity = GetParent().mrig.GetPointVelocity(transform.position);

        transform.SetParent(null);
    }
}
