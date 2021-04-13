using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_SERVER
#else
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
#endif

using Mirror;
using PlayFab;
using System;
using PlayFab.MultiplayerAgent.Model;
public class NetMan : NetworkManager
{
    public OnlineGameControl onlinegameControl;
    private Dictionary<NetworkConnection, int> connections = new Dictionary<NetworkConnection, int>();
    public TelepathyTransport telepathyTransport;


#if UNITY_SERVER
    static private List<PlayFab.MultiplayerAgent.Model.ConnectedPlayer> players = new List<PlayFab.MultiplayerAgent.Model.ConnectedPlayer>();
#endif
    private int connNum = 0;
    private string ticketID;
    private IEnumerator co;
    private readonly static Vector2[] starts = new Vector2[]{
        new Vector2(-10.0f, 10.0f),
        new Vector2(10.0f, -10.0f),
        new Vector2(-10.0f, -10.0f),
        new Vector2(10.0f, 10.0f),
        new Vector2(0.0f, 0.0f)
    };

    private readonly string QueueName = "1v1";

#if UNITY_SERVER

    public override void Start()
    {
        Debug.Log("Starting Server");
        //if (PlayFabMultiplayerAgentAPI == null) Debug.Log("null");
        //if (PlayFab.PlayFabMultiplayerAgentAPI == null) Debug.Log("null");
        PlayFabMultiplayerAgentAPI.Start();
        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA \n\n\n\n\n\n\n\n\n\n\n\n\n");
        PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
        PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
        PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
        PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;
        StartCoroutine(ReadyForPlayers());
        StartCoroutine(ShutdownCheck());
    }
    private IEnumerator Startup()
    {
        yield return new WaitForSeconds(1.0f);
        
    }

    private void OnAgentError(string error)
    {
        Debug.Log(error);
    }

    private void OnServerActive()
    {
        Debug.Log("Started Server");
        StartServer();
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

#else
    public override void Start()
    {
        base.Start();
        LoginRemoteUser();
    }

    public void LoginRemoteUser()
    {
        Debug.Log("[ClientStartUp].LoginRemoteUser");

        //We need to login a user to get at PlayFab API's. 
        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = GUIDUtility.getUniqueID()
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnPlayFabLoginSuccess, OnLoginError);
    }

    private void OnLoginError(PlayFabError response)
    {
        Debug.Log(response.ToString());
    }

    private void OnPlayFabLoginSuccess(LoginResult response)
    {
        Debug.Log(response.ToString());
        Debug.Log("Searching for Game");
        RequestMatch(response);
        
    }

    private void RequestMatch(LoginResult response)
    {
        PlayFabMultiplayerAPI.CreateMatchmakingTicket(
    new CreateMatchmakingTicketRequest
    {
        // The ticket creator specifies their own player attributes.
        Creator = new MatchmakingPlayer
        {
            Entity = new PlayFab.MultiplayerModels.EntityKey
            {
                Id = response.EntityToken.Entity.Id,
                Type = response.EntityToken.Entity.Type,
            },

            // Here we specify the creator's attributes.
            Attributes = new MatchmakingPlayerAttributes
            {
                EscapedDataObject = "{\"Region\" : [{ \"region\": \"NorthEurope\", \"latency\": 150}]}"
            },
        },

        // Cancel matchmaking if a match is not found after 120 seconds.
        GiveUpAfterSeconds = 120,

        // The name of the queue to submit the ticket into.
        QueueName = QueueName,
    },

    // Callbacks for handling success and error.
    this.OnMatchmakingTicketCreated,
    this.OnMatchmakingError);
    }

    private void OnMatchmakingError(PlayFabError obj)
    {
        Debug.Log("Error");
        Debug.Log(obj);
    }

    private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult obj)
    {
        
        ticketID = obj.TicketId;
        Debug.Log("Ticket created");
        Debug.Log(ticketID);
        co = CheckTicketStatus(ticketID);
        StartCoroutine(co);
    }

    private IEnumerator CheckTicketStatus(string ticketID)
    {
        while (true)
        {
            Debug.Log("Checkin in with the gang...");
            
            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest
                {
                    TicketId = ticketID,
                    QueueName = QueueName,
                },
                this.OnGetMatchmakingTicket,
                this.OnMatchmakingError);
            Debug.Log("Done Checkin in");
            yield return new WaitForSeconds(6.0f);
        }
        


    }

    private void OnGetMatchmakingTicket(GetMatchmakingTicketResult obj)
    {
        Debug.Log(obj.Status);
        if (obj.Status == "Matched")
        {
            StopCoroutine(co);
            GetMatch(obj.MatchId);
        }
        else if (obj.Status == "Canceled")
        {
            StopCoroutine(co);
        }
    }

    private void GetMatch(string matchId)
    {
        PlayFabMultiplayerAPI.GetMatch(
        new GetMatchRequest
        {
            MatchId = matchId,
            QueueName = QueueName,
        },
        this.OnGetMatch,
        this.OnMatchmakingError);
    }

    private void OnGetMatch(GetMatchResult obj)
    {
        Debug.Log(obj.ServerDetails);
        //NetworkClient.Connect(obj.ServerDetails.IPV4Address, obj.ServerDetails.Ports[0].Num);
        networkAddress = obj.ServerDetails.IPV4Address;
        
        if(obj.ServerDetails.Ports != null)
        {
            Debug.Log(obj.ServerDetails.Ports.Count);
            if (obj.ServerDetails.Ports.Count > 0)
            {
                foreach (Port a in obj.ServerDetails.Ports)
                {
                    Debug.Log(a.Name);
                    Debug.Log(a.Num);
                    GetComponent<kcp2k.KcpTransport>().Port = (ushort)a.Num;
                }
            }
        }
        
        //telepathyTransport.port = (ushort)obj.ServerDetails.Ports[0].Num;

        StartClient();
    }

#endif
    private IEnumerator ShutdownCheck()
    {
        yield return new WaitForSeconds(300.0f);
        if (connNum <= 0)
        {
            Debug.Log("Shutting Down");
            Application.Quit();
        }
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // TODO: Prevent excess players joining a match
        //       Or mid-game joins

        Debug.Log("Server adding a new player");

        GameObject obj = Instantiate(playerPrefab, starts[connNum % starts.Length], Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, obj);

        int id = onlinegameControl.AddPlayer(obj, conn);
        connections[conn] = id;

        connNum += 1;

        if (connNum == 1) onlinegameControl.ServerStartGame();

#if UNITY_SERVER
        players.Add(new PlayFab.MultiplayerAgent.Model.ConnectedPlayer($"PlayerNumber{id}"));
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(players);
#endif

    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {

        base.OnServerDisconnect(conn);
        int id = 0;
        if(connections.TryGetValue(conn, out id))
        {
            onlinegameControl.RemovePlayer(id);
            connNum -= 1;
#if UNITY_SERVER
        PlayFab.MultiplayerAgent.Model.ConnectedPlayer player = players.Find(x => x.PlayerId.Equals($"PlayerNumber{id}", StringComparison.OrdinalIgnoreCase));
        players.Remove(player);
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(players);
#endif
        }
        if (connNum <= 0) StartCoroutine(ShutdownCheck());
    }

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

    // Called on client when connect to server
    //public override void OnClientConnect(NetworkConnection conn)
    //{
    //    base.OnClientConnect(conn);

    //    gameController.ClientConnected(conn);
    //}
}


