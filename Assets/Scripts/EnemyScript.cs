using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private enum AttackMode
    {
        DIRECT,
        CIRCLE,
        SPIN,
        FLEE,
        // ???
    } private const int attackModeCount = 4;
    private float[] times = new float[] { 6.0f, 2.5f, 6.0f, 1.0f };
    
    public RobotScript mover;
    Rigidbody2D mrig;

    PlayerScript player;

    // Start is called before the first frame update
    void Start()
    {
        mrig = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerScript>();

        StartCoroutine(ChangeWeapon());
        StartCoroutine(ChangeAttackMethod());
        StartCoroutine(UseThing());
    }
    
    private float weaponChoice = 0.0f;

    private AttackMode mode;

    IEnumerator ChangeWeapon()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        while (true)
        {
            // DEFENSIVE
            //if (Random.Range(0.0f, 1.0f) < 0.5f)  // -> Do it when on low life?
            //{
            //    Vector2 os = (Vector2)transform.InverseTransformPoint(GetComponent<RobotScript>().GetControlPos()) - mrig.centerOfMass;
            //    if (Mathf.Abs(os.x) < 0.1f) os.x = 0.1f;
            //    weaponChoice = Mathf.Atan2(os.y, os.x) - Mathf.PI / 2.0f;
            //    continue;
            //}

            List<int> weapons = new List<int>();
            for (int i=0; i<transform.childCount; i++)
            {
                Transform p = transform.GetChild(i);
                if (p.GetComponent<WeaponBlock>() != null)
                {
                    weapons.Add(i);
                }
            }
            if (weapons.Count == 0) break;

            // pi/2 -> 0
            // pi -> pi/2
            // 0 -> 

            int choose = weapons[Random.Range(0, weapons.Count)];
            Vector2 pos = transform.GetChild(choose).localPosition;
            Vector2 offset = pos - mrig.centerOfMass;
            if (Mathf.Abs(offset.x) < 0.1f) offset.x = 0.1f;
            weaponChoice = Mathf.PI / 2.0f - Mathf.Atan2(offset.y, offset.x);


            yield return new WaitForSeconds(Random.Range(1.0f, 5.0f));
        }
    }

    // todo: call, write
    IEnumerator ChangeAttackMethod()
    {
        while (true)
        {
            int a = (int)mode;
            int b = Random.Range(0, attackModeCount - 1);
            if (b >= a) b++;
            mode = (AttackMode)b;
            yield return new WaitForSeconds(times[(int)mode] * Random.Range(0.8f, 1.2f));
        }
    }

    IEnumerator UseThing()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            if (player == null) break;

            if (mode == AttackMode.DIRECT || mode == AttackMode.SPIN)
            {
                Vector2 playerPos = player.mover.GetControlPos();
                Vector2 mPos = mrig.worldCenterOfMass;

                if (Vector2.Distance(playerPos, mPos) < 18.0f)
                {
                    // TODO: This won't work online
                    mover.Use();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        switch (mode)
        {
            case AttackMode.CIRCLE:
                MoveCircle();
                break;
            case AttackMode.DIRECT:
                MoveDirect();
                break;
            case AttackMode.SPIN:
                MoveSpin();
                break;
            case AttackMode.FLEE:
                MoveFlee();
                break;
        }
    }

    // Move away, but still point weapon at us
    private void MoveFlee()
    {
        Vector2 playerPos = player.mover.GetControlPos();
        Vector2 mPos = mrig.worldCenterOfMass;

        Vector2 move = (mPos - playerPos).normalized;

        // Detect dist to wall
        float maxdist = Physics2D.Raycast(mPos + move * 10, move).distance;
        if (maxdist < mrig.velocity.magnitude)
        {
            // Cornered - attack
            mode = AttackMode.DIRECT;
            MoveDirect();
        }

        Vector2 off = playerPos - mPos;
        float ang = weaponChoice + Mathf.Atan2(off.y, off.x);
        Vector2 turn = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

        mover.Move(move, turn);
    }

    private bool dir = true;
    private void MoveCircle()
    {
        Vector2 playerPos = player.mover.GetControlPos();
        Vector2 mPos = mrig.worldCenterOfMass;

        if (Vector2.Distance(playerPos, mPos) < 15.0f)
        {
            MoveFlee();
        }
        else
        {
            Vector2 off = Vector2.Perpendicular(playerPos - mPos).normalized;
            if (dir) off = -off;

            float maxdist = Physics2D.Raycast(mPos + off * 10, off).distance;
            if (maxdist < mrig.velocity.magnitude)
            {
                dir = !dir;
                MoveDirect();
            }
            else
            {
                Vector2 off2 = playerPos - mPos;
                float ang = weaponChoice + Mathf.Atan2(off2.y, off2.x);
                Vector2 turn = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

                mover.Move(off, turn);
            }
        }
    }

    private void MoveSpin()
    {
        Vector2 playerPos = player.mover.GetControlPos();
        Vector2 mPos = mrig.worldCenterOfMass;

        if (Vector2.Distance(playerPos, mPos) < 15.0f)
        {
            float ang = (mrig.rotation + 179.0f) * Mathf.Deg2Rad;
            Vector2 turn = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

            mover.Move((playerPos - mPos).normalized, turn);
        }
        else
        {
            MoveDirect();
        }
    }

    private void MoveDirect()
    {
        Vector2 playerPos = player.mover.GetControlPos();
        Vector2 mPos = mrig.worldCenterOfMass;

        Vector2 off = playerPos - mPos;
        float ang = weaponChoice + Mathf.Atan2(off.y, off.x);
        Vector2 turn = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

        Debug.DrawLine(mPos, mPos + turn * 5.0f);

        mover.Move((playerPos - mPos).normalized, turn);
    }
}
