using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FAKENETMAN : NetworkManager
{
    public override void Awake()
    {
        Controller.isLocalGame = true;

        base.Start();
        //StartServer();
        StartHost();
        NetworkServer.dontListen = true;
    }

    //public override void OnServerAddPlayer(NetworkConnection conn)
    //{
    //    Debug.LogError("Lmao this should not happen");
    //    conn.Disconnect();
    //}
    
    //public override void OnClientConnect(NetworkConnection conn)
    //{
    //    Debug.LogError("Fuck ooffff");
    //    conn.Disconnect();
    //}
}


