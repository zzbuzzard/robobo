using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetMan : NetworkManager
{
    public OnlineGameControl onlinegameControl;

    private bool first = true;
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("Server add player");

        GameObject obj = Instantiate(playerPrefab, new Vector2(10.0f, 10.0f)*(first?1:-1), Quaternion.identity);
        first = false;
        NetworkServer.AddPlayerForConnection(conn, obj);

        onlinegameControl.AddPlayer(obj, conn);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        NetworkClient.AddPlayer();

        Debug.Log("Client connected, requested a player add");
    }

    // Called on client when connect to server
    //public override void OnClientConnect(NetworkConnection conn)
    //{
    //    base.OnClientConnect(conn);

    //    gameController.ClientConnected(conn);
    //}
}


