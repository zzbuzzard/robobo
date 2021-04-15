//#define LOCAL_TEST
// Dont forget the LOCAL_TEST in PlayFabHandlerClient!

// TODO: Try to move all PlayFab stuff out of here
//  We don't really want to mix Mirror and PlayFab too much...
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.MultiplayerAgent.Model;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFab;
using Mirror;

public class NetMan : NetworkManager
{
    public OnlineGameControl onlinegameControl;

#if UNITY_SERVER
    // Spawn points and counter for next spawnpoint
    private readonly static Vector2[] starts = new Vector2[]{
        new Vector2(-10.0f, 10.0f),
        new Vector2(10.0f, -10.0f),
        new Vector2(-10.0f, -10.0f),
        new Vector2(10.0f, 10.0f),
        new Vector2(0.0f, 0.0f)
    };

    // When the server stops, end the process
    // NOTE: OnStopServer() is only called when StopServer() is run - 
    //       Most of the time the process is ended by OnlineGameControl.ServerEndGame()
    private void Terminate()
    {
        Debug.Log("Shutting Down");
        Application.Quit();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Terminate();
    }

    // NON-PLAYFAB STUFF:
#if LOCAL_TEST
    public override void Start()
    {
        Debug.Log("Local server starting");
        StartServer();
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("Server adding a new player");
        int connNum = onlinegameControl.NumberOfPlayers();
        GameObject obj = Instantiate(playerPrefab, starts[connNum % starts.Length], Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, obj);
        onlinegameControl.AddPlayer(obj, conn);
    }

// PLAYFAB STUFF:
#else
    private Dictionary<NetworkConnection, int> connectionToID = new Dictionary<NetworkConnection, int>();

    // TODO: I removed static - is this ok?
    private List<PlayFab.MultiplayerAgent.Model.ConnectedPlayer> players = new List<PlayFab.MultiplayerAgent.Model.ConnectedPlayer>();

    public override void Start()
    {
        // TODO: base.Start() ?

        Debug.Log("Requesting server start");
        PlayFabMultiplayerAgentAPI.Start();

        PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
        PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
        PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
        PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

        StartCoroutine(ReadyForPlayers());
        StartCoroutine(ShutdownCheck());
    }

    private void OnAgentError(string error) => Debug.Log(error);
    
    private void OnServerActive()
    {
        Debug.Log("Started Server");
        StartServer(); // TODO: Wait for NetworkManager.OnStartServer() to run before calling ReadyForPlayers()?
    }

    private void OnShutdown()
    {
        Debug.Log("Stopped Server");
        StopServer();
    }

    private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
    {
        Debug.LogFormat("Maintenance Scheduled for: {0}", NextScheduledMaintenanceUtc.Value.ToLongDateString());
    }

    private IEnumerator ReadyForPlayers()
    {
        yield return new WaitForSeconds(.5f);
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
        Debug.Log("Ready For Players!!!!!!!!");
    }

    private IEnumerator ShutdownCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(180.0f);
            if (onlinegameControl.NumberOfPlayers() == 0)
            {
                Terminate();
            }
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // TODO: Prevent excess players joining a match
        //       Or mid-game joins

        Debug.Log("Server adding a new player");

        int connNum = onlinegameControl.NumberOfPlayers();

        GameObject obj = Instantiate(playerPrefab, starts[connNum % starts.Length], Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, obj);

        int id = onlinegameControl.AddPlayer(obj, conn);
        connectionToID[conn] = id;
        
        players.Add(new PlayFab.MultiplayerAgent.Model.ConnectedPlayer($"PlayerNumber{id}"));
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(players);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        int id = 0;

        if (connectionToID.TryGetValue(conn, out id))
        {
            onlinegameControl.RemovePlayer(id);

            // TODO:
            //  Wouldn't a dictionary int (id) -> obj be better?
            //  Could even store along with the NetworkConnection (connectionToID)
            PlayFab.MultiplayerAgent.Model.ConnectedPlayer player = players.Find(x => x.PlayerId.Equals($"PlayerNumber{id}", StringComparison.OrdinalIgnoreCase));
            players.Remove(player);
            PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(players);

            connectionToID.Remove(conn);
        }
    }
#endif // end playfab stuff
#endif // end server stuff

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        Shutdown();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        NetworkClient.AddPlayer();

        Debug.Log("Client connected, requested a player add");
    }
}


