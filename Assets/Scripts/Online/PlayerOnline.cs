using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// 1) Connects to server
// 2) Commands server to spawn my robot
// 3) Blocks use SyncVar to set their x, y etc.
// 4) Check all blocks each step - wait til all x, y are initialised.
// 5) Once all initialised, enable RobotScript and PlayerScript
// 6) 
public class PlayerOnline : NetworkBehaviour
{
    private Robot myRobot;

    // Called when the player is spawned in, on the client
    // Purpose: send our robot to the server
    public override void OnStartLocalPlayer()
    {
        GameObject.Find("GameController").GetComponent<GameController>().SetPlayer(gameObject);

        // Ask server to please spawn our robot
        CmdSpawnPlayerRobot(Robot.SerializeRobot(Controller.playerRobot));
    }

    // Runs on server
    [Command]
    private void CmdSpawnPlayerRobot(Robot.SerializedRobot sr)
    {
        // TODO: Check not cheating
        Robot r = Robot.DeserializeRobot(sr);

        // This method also spawns onto clients
        GetComponent<RobotScript>().LoadRobot(r);

        ClientSetRobot(sr);
    }

    [ClientRpc]
    private void ClientSetRobot(Robot.SerializedRobot sr)
    {
        myRobot = Robot.DeserializeRobot(sr);
    }

    private bool isReady = false;
    private void Update()
    {
        if (isReady) return;
        if (myRobot == null) return;
        if (transform.childCount != myRobot.blockTypes.Count) return; // Not all spawned yet

        bool works = true;
        for (int i=0; i<transform.childCount; i++)
        {
            Block b = transform.GetChild(i).GetComponent<Block>();
            if (!b.IsInitialisedByServer())
            {
                works = false;
                break;
            }
        }

        if (works)
        {
            isReady = true;

            GetComponent<RobotScript>().LoadRobotClient(myRobot);
            GetComponent<RobotScript>().enabled = true;
            if (isLocalPlayer)
                GetComponent<PlayerScript>().enabled = true;
        }
    }
}