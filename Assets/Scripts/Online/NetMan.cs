using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetMan : NetworkManager
{
    // Called on server when this player connects
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
    }

    // Called on client when connect to server
    //public override void OnClientConnect(NetworkConnection conn)
    //{
    //    base.OnClientConnect(conn);

    //    gameController.ClientConnected(conn);
    //}
}


