using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetMan : NetworkManager
{
    public OnlineGameControl onlinegameControl;

    private int connNum = 0;
    private readonly static Vector2[] starts = new Vector2[]{
        new Vector2(-10.0f, 10.0f),
        new Vector2(10.0f, -10.0f),
        new Vector2(-10.0f, -10.0f),
        new Vector2(10.0f, 10.0f),
        new Vector2(0.0f, 0.0f)
    };

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // TODO: Prevent excess players joining a match
        //       Or mid-game joins

        Debug.Log("Server adding a new player");

        GameObject obj = Instantiate(playerPrefab, starts[connNum % starts.Length], Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, obj);

        onlinegameControl.AddPlayer(obj, conn);

        connNum += 1;
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


