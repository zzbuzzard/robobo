using System.Collections;
using UnityEngine;

//using PlayFab.MultiplayerAgent.Model;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFab;

// Client-only class that attempts to find a match via PlayFab
public class PlayFabHandlerClient : MonoBehaviour
{
    public NetMan netMan;

#if UNITY_SERVER
#else

    private string ticketID;
    private readonly string QueueName = "1v1";

    private IEnumerator co;

    private void Start()
    {
        LoginRemoteUser();
    }

    // Begin the matchmaking process by logging in
    private void LoginRemoteUser()
    {
        Debug.Log("Attempting PlayFab login");

        //We need to login a user to get at PlayFab API's. 
        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = GUIDUtility.GetUniqueID()
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnPlayFabLoginSuccess, OnLoginError);
    }

    private void OnPlayFabLoginSuccess(LoginResult response)
    {
        Debug.Log("PlayFab login succeeded: " + response);
        RequestMatch(response);
    }

    private void RequestMatch(LoginResult response)
    {
        Debug.Log("Requesting a match");

        // Create matchmaking ticket
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

    private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult obj)
    {
        ticketID = obj.TicketId;
        Debug.Log("Ticket created: " + ticketID);
        co = CheckTicketStatus(ticketID);
        StartCoroutine(co);
    }

    private void OnGetMatchmakingTicket(GetMatchmakingTicketResult obj)
    {
        Debug.Log("Matchmaking ticket status: " + obj.Status);
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
        this.OnGetMatchError);
    }

    private void OnGetMatch(GetMatchResult obj)
    {
        Debug.Log("Server details: " + obj.ServerDetails.ToString());
        foreach (MatchmakingPlayerWithTeamAssignment a in obj.Members)
        {
            Debug.Log("Teammate: " + a.Entity.Id);
        }

        netMan.networkAddress = obj.ServerDetails.IPV4Address;

        if (obj.ServerDetails.Ports != null)
        {
            if (obj.ServerDetails.Ports.Count > 0)
            {
                foreach (Port a in obj.ServerDetails.Ports)
                {
                    GetComponent<kcp2k.KcpTransport>().Port = (ushort)a.Num;
                }
            }
        }

        //telepathyTransport.port = (ushort)obj.ServerDetails.Ports[0].Num;

        netMan.StartClient();
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


    ////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    /////////////////////////////////    Error callbacks   /////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////

    private void OnMatchmakingError(PlayFabError response)
    {
        Debug.Log("Matchmaking error: " + response.ToString());
    }

    private void OnLoginError(PlayFabError response)
    {
        Debug.Log("PLAYFAB LOGIN FAILED: " + response.ToString());
    }

    private void OnGetMatchError(PlayFabError response)
    {
        Debug.Log("Get match error: " + response.ToString());
    }
#endif
}
