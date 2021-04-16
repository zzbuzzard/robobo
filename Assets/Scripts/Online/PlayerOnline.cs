using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// 1) Connects to server
// 2) Commands server to spawn my robot
// 3) Blocks use SyncVar to set their x, y etc.
// 4) Check all blocks each step - wait til all x, y are initialised.
// 5) Once all initialised, enable RobotScript and PlayerScript
public class PlayerOnline : NetworkBehaviour
{
    [SyncVar]
    public int myID = -1;

    public Robot myRobot { get; private set; }

    // Until isReady, we loop and wait for our blocks to spawn.
    public bool isReady { get; private set; } = false;

    // Runs on server
    [Command]
    private void CmdSpawnPlayerRobot(Robot.SerializedRobot sr)
    {
        // TODO: Check not cheating
        myRobot = Robot.DeserializeRobot(sr);

        // This method also spawns blocks onto clients, but further intialisation is needed
        GetComponent<RobotScript>().LoadRobot(myRobot);

        ClientSetRobot(sr);
    }

    [ClientRpc]
    private void ClientSetRobot(Robot.SerializedRobot sr)
    {
        myRobot = Robot.DeserializeRobot(sr);
    }

    // Called on a client who is late
    [TargetRpc]
    public void LateLoad(NetworkConnection conn, Robot.SerializedRobot sr)
    {
        myRobot = Robot.DeserializeRobot(sr);
    }

#if UNITY_SERVER
#else
    // Called when the player is spawned in, on the client
    // Purpose: send our robot to the server
    public override void OnStartLocalPlayer()
    {
        if (Controller.isLocalGame) return;

        base.OnStartLocalPlayer();

        GameObject.Find("GameController").GetComponent<GameController>().SetPlayer(gameObject);
        GameObject.Find("OnlineController").GetComponent<OnlineGameControl>().SetPlayer(gameObject);

        // Ask server to please spawn our robot
        CmdSpawnPlayerRobot(Robot.SerializeRobot(Controller.playerRobot));
    }


    private void Update()
    {
        if (isReady) return;
        if (myID == -1) return;
        if (myRobot == null) return;
        if (transform.childCount != myRobot.blockTypes.Count) return; // Not all spawned yet

        bool works = true;
        for (int i = 0; i < transform.childCount; i++)
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
            GetComponent<RobotScript>().LoadRobotClient(myRobot);
            GetComponent<RobotScript>().enabled = true;
            GetComponent<InterpolateController>().Initialise();

            if (isLocalPlayer)
            {
                GetComponent<InterpolateController>().isLocal = true;
                GetComponent<PlayerScript>().enabled = true;
            }

            Debug.Log("Spawned player " + myID + " is now ready");
            isReady = true;
        }
    }
#endif


    /*
    Deprecated latency test thing. Note: It didn't work that well locally, and the first one was always 10x higher than the rest.

    // Latency, in fixed update frames
    public static int twoL { get; set; } = 5;

    private int l;
    private bool testingL=false;

    public void UpdateLatency()
    {
        l = 0;
        testingL = true;

        CmdLatencyTest();
    }

    [Command]
    private void CmdLatencyTest()
    {
        LatencyTestResturned();
    }

    [TargetRpc]
    private void LatencyTestResturned()
    {
        testingL = false;
        twoL = 1 + l;
        Debug.Log("Latency rt is about " + twoL + " frames");
    }

    private void FixedUpdate()
    {
        if (testingL) l++;
    }

    IEnumerator RefreshLatency()
    {
        while (connectionToServer != null)
        {
            UpdateLatency();
            yield return new WaitForSeconds(5.0f);
        }
    }
    */
}