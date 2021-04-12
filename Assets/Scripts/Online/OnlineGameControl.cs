using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;

class GameState
{
    public int frameID;
    public int[] ids;  // IDs of the rigidbodies
    public Vector2[] rigPos;
    public Vector2[] rigVel;
    public float[] rigAngVel;
    public float[] rotations;

    public static bool IsSignificantlyDifferent(GameState a, GameState b)
    {
        const float velDif = 0.1f,
                    angVelDif = 15.0f, // degrees per sec
                    posDif = 0.5f,
                    rotDif = 10.0f;     // degrees

        // Different robots??
        if (a.ids.Length != b.ids.Length)
        {
            return true;
        }

        // Compare each robot
        for (int i=0; i<a.ids.Length; i++)
        {
            // Square distance much faster to compute
            float sqdist = (a.rigPos[i].x - b.rigPos[i].x) * (a.rigPos[i].x - b.rigPos[i].x) +
                (a.rigPos[i].y - b.rigPos[i].y) * (a.rigPos[i].y - b.rigPos[i].y);
            if (sqdist > posDif)
            {
                Debug.Log("Position difference");
                return true;
            }

            // TODO: Better system for checking velocity difference needed
            if (Mathf.Abs(a.rigVel[i].x - b.rigVel[i].x) > velDif || Mathf.Abs(a.rigVel[i].y - b.rigVel[i].y) > velDif)
            {
                Debug.Log("Velocity difference");
                return true;
            }

            if (Mathf.Abs(a.rigAngVel[i] - b.rigAngVel[i]) > angVelDif)
            {
                Debug.Log("Angular velocity difference: diff is " + Mathf.Abs(a.rigAngVel[i] - b.rigAngVel[i]));
                return true;
            }

            // TODO: Account for 360 degree difference (359 and 0: not sig dif)
            if (Mathf.Abs(a.rotations[i] - b.rotations[i]) > rotDif)
            {
                Debug.Log("Rotation difference: diff is " + Mathf.Abs(a.rotations[i] - b.rotations[i]));
                return true;
            }
        }

        return false;
    }
}

// TODO:
// Consider removing inputFrame from here,
//  and storing it instead in a dictionary on the server
//  (slower than a buffer, surely? could use buffer as a dict?)
struct InputPkg
{
    // turn is Vector2.zero if no turn
    public Vector2 move, turn;
    //public bool useWeapon;
    public int inputFrame;

    public static InputPkg AveragePkg(InputPkg a, InputPkg b)
    {
        // Note: Safe to modify a and b, they are passed by value (structs)
        // Note: Could pass b by reference

        // 1) Average turn (or only take one if the other is 0)
        a.turn += b.turn;
        if (a.turn != Vector2.zero && b.turn != Vector2.zero)
            a.turn /= 2;

        // 2) Average move
        a.move = (a.move + b.move) / 2;

        // 3) Average useWeapon (use if either do)
        //a.useWeapon |= b.useWeapon;

        a.inputFrame = (a.inputFrame + b.inputFrame) / 2;
        return a;
    }

    public static bool IsInputPkgDifferent(InputPkg a, InputPkg b)
    {
        const float threshold = 0.01f;

        //if (a.useWeapon != b.useWeapon) return true;
        if (Mathf.Abs(a.move.x - b.move.x) > threshold) return true;
        if (Mathf.Abs(a.move.y - b.move.y) > threshold) return true;
        if (Mathf.Abs(a.turn.x - b.turn.x) > threshold) return true;
        if (Mathf.Abs(a.turn.y - b.turn.y) > threshold) return true;
        return false;
    }

    public override string ToString()
    {
        string s = "Frame " + inputFrame + ": " + "Move " + move + ", turn " + turn;
        //if (useWeapon) s += " (USE)";
        return s;
    }
}


// One instance on each client, and on the server
// Client: Sends state, checks server's and sometimes reverts physics
// Server: Receives inputs, moves rigidbodies
public class OnlineGameControl : NetworkBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    /////////////////////////////////      Server only     /////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////

    //SyncDictionary<int, InputPkg> lastPlayerInputDict = new SyncDictionary<int, InputPkg>();
        
    // TODO: Swap this queue for a buffer of fixed size for speed. Profile it.
    IDictionary<int, float> playerSpeedUpValues;
    IDictionary<int, Queue<InputPkg>> queuedPlayerInputs;
    IDictionary<int, InputPkg> lastPlayerInput;
    const int queueSizeThreshold = 5; // Beyond this size, we do two at once
    private int nextID = 0;

    private void Awake()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;

        // TODO: Split into server/client
        // NOTE: Can't use isServer at this point, as it doesn't know yet. Need a public bool or something.

        players = new Dictionary<int, GameObject>();
        queuedPlayerInputs = new Dictionary<int, Queue<InputPkg>>();
        playerSpeedUpValues = new Dictionary<int, float>();
        pastGameStates = new Queue<GameState>();
        pastInputs = new Dictionary<int, InputPkg>();
        lastPlayerInput = new Dictionary<int, InputPkg>();
    }

    [Server]
    public int AddPlayer(GameObject player, NetworkConnection conn)
    {
        // First, tell this player about the players who joined before it
        foreach (var pair in players)
        {
            PlayerOnline p = pair.Value.GetComponent<PlayerOnline>();
            p.LateLoad(conn, Robot.SerializeRobot(p.myRobot));
            LateClientAddPlayer(conn, pair.Key, pair.Value);
        }

        player.GetComponent<PlayerOnline>().myID = nextID;
        players[nextID] = player;
        queuedPlayerInputs[nextID] = new Queue<InputPkg>();
        lastPlayerInput[nextID] = default;
        //lastPlayerInputDict.Add(nextID, default);
        playerSpeedUpValues[nextID] = 0.0f;

        // Tell all pre-existing clients, as well as the new client, to add this player
        ClientAddPlayer(nextID, player);

        

        nextID++;

        return nextID - 1;
    }
    
    // Called when a player DCs or dies
    [Server]
    public void RemovePlayer(int id)
    {
        if (players.ContainsKey(id))
        {
            GameObject player = players[id];
            players.Remove(id);

            if (player != null)
            {
                Destroy(player);
            }

            lastPlayerInput.Remove(id);
            queuedPlayerInputs.Remove(id);
            playerSpeedUpValues.Remove(id);
        }

        ClientRemovePlayer(id);

        CheckWin();
    }

    // Check for deleted GameObjects - these will have DCed, or been killed.
    [Server]
    private void RemoveDead()
    {
        // ToList() - may be modified during iter
        foreach (var pair in players.ToList())
        {
            if (pair.Value == null)
                RemovePlayer(pair.Key);
        }
    }

    [Server]
    private void CheckWin()
    {
        if (players.Count == 1)
        {
            int winID = players.Keys.ToList()[0];

            gameRunning = false;
            Debug.Log("PLAYER " + winID + " HAS WON");

            // Broadcast to all clients that the game is over, won by winID
            ClientEndGame(winID);
        }
    }

    // TODO: Call this somewhere better [plan]
    // Server starts game
    [Server]
    public void ServerStartGame()
    {
        Debug.Log("Server: Starting game");

        frameOn = 0;
        gameRunning = true;

        StartCoroutine(SpeedUpSlowDownRoutine());

        ClientStartGame();
    }

    // Apply inputs on server
    [Server]
    private void ProcessInputs()
    {
        foreach (var pair in queuedPlayerInputs)
        {
            InputPkg use;

            int id = pair.Key;

            // TODO: Prevent queue becoming very large.

            // No queued moves ... they should speed up so we have a backlog
            if (pair.Value.Count == 0)
            {
                use = lastPlayerInput[id];

                // SPEED UP
                playerSpeedUpValues[id] = playerSpeedUpValues[id] / 2.0f + 1.0f;
            }
            else
            {
                int nextInputTime = pair.Value.Peek().inputFrame;

                // We haven't reached the frame for the next input yet
                // WAIT: I don't think this actually ever happens
                if (nextInputTime > frameOn)
                {
                    use = lastPlayerInput[id];

                    // SLOW DOWN
                    playerSpeedUpValues[id] = playerSpeedUpValues[id] / 2.0f - 1.0f;
                }
                else {
                    // We should've applied it in the past!
                    if (nextInputTime < frameOn)
                    {
                        if (pair.Value.Count > 1)
                            use = InputPkg.AveragePkg(pair.Value.Dequeue(), pair.Value.Dequeue());
                        else
                            use = pair.Value.Dequeue();

                        // SPEED UP
                        playerSpeedUpValues[id] = playerSpeedUpValues[id] / 2.0f + 1.0f;
                    }

                    // Perfect... They're equal
                    else
                    {
                        use = pair.Value.Dequeue();

                        // THEY'RE FINE
                        playerSpeedUpValues[id] = playerSpeedUpValues[id] / 2.0f;
                    }
                }

                //// Too many queued moves, use 2
                //if (pair.Value.Count >= queueSizeThreshold)
                //{
                //    Debug.LogWarning("Too many moves queued for player " + id + ": " + pair.Value.Count);
                //    use = InputPkg.AveragePkg(pair.Value.Dequeue(), pair.Value.Dequeue());
                //}
                //// The right number, use one
                //else
                //{
                //    use = pair.Value.Dequeue();
                //}
            }

            //Debug.Log("MOVE " + use.inputFrame + " ON " + frameOn
            //    + "\n(Number of other queued moves: " + pair.Value.Count + ")"
            //    + (pair.Value.Count > 0 ? "\n(Queue starts at " + pair.Value.Peek().inputFrame + ")" : "")
            //    );

            lastPlayerInput[id] = use;
            // This reduces the number of calls
            //if (InputPkg.IsInputPkgDifferent(use, lastPlayerInputDict[id]))
            //    lastPlayerInputDict[id] = use;

            // Apply this package
            ApplyInputPackage(players[id], use);
        }
    }

    // Client -> Server sends input
    [Command(requiresAuthority = false)]
    private void ServerReceiveInput(int myID, InputPkg clientInput)
    {
        if (!gameRunning) return;
        if (!players.ContainsKey(myID)) return;

        // TODO: Ensure they actually arrive in order!
        //  (Note: They seem to)
        // TODO: Ensure the frame ID hasn't been fucked with
        //   -> This won't play to their advantage really, just should move the frame check from ProcessInputs to here
        //      This will prevent a queue building up of frames which we're just gonna discard anyway
        queuedPlayerInputs[myID].Enqueue(clientInput);
    }

    [Server]
    private void SpeedUpSlowDown()
    {
        // TODO: Kinda workaround slow ToList() call
        foreach (int id in playerSpeedUpValues.Keys.ToList())
        {
            Debug.Log("Speed up slow down: Queue size for " + id + " is " + queuedPlayerInputs[id].Count);

            // If they're not being told to speed up (so they're not super laggy)
            // Then tell them to slow down
            if (playerSpeedUpValues[id] <= 0.01f && queuedPlayerInputs[id].Count >= 5)
                playerSpeedUpValues[id] = playerSpeedUpValues[id] / 2 - 1.0f;
        }

        foreach (var pair in playerSpeedUpValues)
        {
            int id = pair.Key;
            float val = pair.Value;

            // TargetRPC
            SetSpeedMul(players[id].GetComponent<NetworkIdentity>().connectionToClient, val);
        }
    }

    IEnumerator SpeedUpSlowDownRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (gameRunning)
            {
                SpeedUpSlowDown();
            }
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    /////////////////////////////////      Client only     /////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    public TextMeshProUGUI winText;
    IDictionary<int, InputPkg> pastInputs; // MY past inputs
    Queue<GameState> pastGameStates; // the *client* gamestates 
    private GameObject mPlayer;

    public static bool isResimulating = false;

    private GameState waitingState;
    private int lastServerFrame = 0; // the frame number last received from the server

    [TargetRpc]
    private void SetSpeedMul(NetworkConnection conn, float m)
    {
        if (Mathf.Abs(m) > 0.01f)
        {
            Debug.Log("Speed multiplier: " + m);

            // How many frames to skip
            int count = 1;
            if (Mathf.Abs(m) > 0.5f)
                // Not perfect, as the currentServerFrame will be higher
                count = 1 + Mathf.Abs(lastServerFrame - frameOn);

            // Speed up
            if (m > 0)
            {
                for (int i=0; i<count; i++)
                    SimulateFrame();
            }
            // Slow down
            else
            {
                for (int i=0; i<count; i++)
                    ReverseTime();
            }
        }
    }

    [Client]
    private void LoadGameState(GameState s)
    {
        pastGameStates.Clear();
        frameOn = s.frameID;
        pastGameStates.Enqueue(s);

        // TODO: Some kinda visual-only interpolation

        int n = s.ids.Length;
        for (int i=0; i<n; i++)
        {
            GameObject g = players[s.ids[i]];
            if (g == null) continue;

            Rigidbody2D r = g.GetComponent<Rigidbody2D>();
            r.angularVelocity = s.rigAngVel[i];
            r.velocity = s.rigVel[i];
            r.rotation = s.rotations[i];
            r.position += s.rigPos[i] - r.worldCenterOfMass;
        }
    }

    [Client]
    private int GetQueueStart()
    {
        if (pastGameStates.Count == 0) return frameOn;
        return pastGameStates.Peek().frameID;
    }

    [Client]
    private void SimulateFrame()
    {
        // TODO: Track fixed update... and other fixed update...
        frameOn++;
        if (pastInputs.ContainsKey(frameOn))
        {
            ApplyInputPackage(mPlayer, pastInputs[frameOn]);
        }
        else
        {
            Debug.Log("No input package for frame " + frameOn);
        }

        // Apply past inputs to all other players!
        //int mid = mPlayer.GetComponent<PlayerOnline>().myID;
        //foreach (int id in lastPlayerInputDict.Keys)
        //{
        //    if (id != mid)
        //    {
        //        //Debug.Log("Past input: " + lastPlayerInputDict[id].ToString());
        //        //ApplyInputPackage(players[id], lastPlayerInputDict[id]);
        //    }
        //}

        Physics2D.Simulate(Time.fixedDeltaTime);
        pastGameStates.Enqueue(GetCurrentState());
    }

    [Client]
    private void ReverseTime()
    {
        // TODO: Prevent size going below 2 (from server packets, at least)
        if (pastGameStates.Count < 2)
        {
            Debug.LogWarning("Not enough history to reverse time :(");
            return;
        }

        // TODO: Use a buffer or a deque and un-workaround this

        // (Currently, moves to second last in queue, and removes the last from the queue)
        GameState[] arr = pastGameStates.ToArray();
        LoadGameState(arr[arr.Length - 2]);
        pastGameStates.Clear();
        for (int i = 0; i <= arr.Length - 2; i++)
            pastGameStates.Enqueue(arr[i]);

        frameOn--;
    }

    [ClientRpc]
    private void ClientAddPlayer(int playerID, GameObject player)
    {
        players[playerID] = player;
    }

    [ClientRpc]
    private void ClientRemovePlayer(int playerID)
    {
        Debug.Log("Client removing player " + playerID);
        players.Remove(playerID);
    }

    // Adding players who joined before me
    [TargetRpc]
    private void LateClientAddPlayer(NetworkConnection conn, int playerID, GameObject player)
    {
        players[playerID] = player;
    }

    [Client]
    public void SetPlayer(GameObject obj)
    {
        mPlayer = obj;
    }

    [ClientRpc]
    public void ClientStartGame()
    {
        Debug.Log("Client: Starting game");

        frameOn = 0;
        gameRunning = true;
    }

    [ClientRpc]
    public void ClientEndGame(int winner)
    {
        Debug.Log("Game ends: Winner was ID " + winner);

        winText.gameObject.SetActive(true);

        if (mPlayer == null || mPlayer.GetComponent<PlayerOnline>().myID != winner)
        {
            winText.color = Color.red;
            winText.SetText("YOU LOSE");
        }
        else
        {
            winText.color = Color.green;
            winText.SetText("YOU WINNNN");
        }
        gameRunning = false;
    }

    [ClientRpc]
    private void ClientReceiveState(GameState state)
    {
        waitingState = state;
        lastServerFrame = state.frameID;

        // May not be in there, but that's fine (it just returns false)
        pastInputs.Remove(lastServerFrame - 1);
    }
    
    //[Client]
    //private void DebugSimulateFromStart()
    //{
    //    if (pastGameStates.Count == 0) return;
    //    // Re-simulate it all.
    //    int realFrame = frameOn;

    //    LoadGameState(pastGameStates.Dequeue());

    //    Debug.Log("Test simulating " + frameOn + " -> " + realFrame);

    //    while (frameOn < realFrame)
    //    {
    //        SimulateFrame();
    //    }
    //}

    [Client]
    private void CheckPastState()
    {
        if (waitingState == null) return;

        //Debug.Log(waitingState.frameID + " to " + frameOn);

        if (waitingState.frameID < GetQueueStart())
        {
            Debug.Log("Frame " + waitingState.frameID + " before our history, skipping");
            return;
        }

        // We need to get more ahead! Server should be telling us this...
        if (waitingState.frameID > frameOn)
        {
            Debug.Log("Server is ahead! Frameon: " + frameOn + ", waiting state frame: " + waitingState.frameID);
            return;
        }

        // Move through game states until we find the one for the waitingState
        while (pastGameStates.Count > 0 && GetQueueStart() != waitingState.frameID)
        {
            pastGameStates.Dequeue();
        }
        
        if (GetQueueStart() != waitingState.frameID || pastGameStates.Count == 0)
        {
            Debug.LogError("This should never happen!!");
            return;
        }

        GameState mState = pastGameStates.Dequeue();

        if (GameState.IsSignificantlyDifferent(mState, waitingState))
        {
            Debug.Log("SIGNIFICANTLY DIFFERENT: Frame " + mState.frameID
                +"\nSimulating " + mState.frameID + " -> " + frameOn);

            isResimulating = true;

            // Re-simulate it all.
            int realFrame = frameOn;

            LoadGameState(waitingState);

            while (frameOn < realFrame)
            {
                SimulateFrame();
            }

            isResimulating = false;
        }
    }



    ////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    /////////////////////////////////        Shared        /////////////////////////////////
    /////////////////////////////////                      /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////
    IDictionary<int, GameObject> players;

    private bool gameRunning = false;
    int frameOn = 0;
        
    private GameState GetCurrentState()
    {
        GameState state = new GameState();
        state.frameID = frameOn;

        int N = players.Count;
        state.ids = new int[N];
        state.rigPos = new Vector2[N];
        state.rigVel = new Vector2[N];
        state.rigAngVel = new float[N];
        state.rotations = new float[N];

        int ind = 0;
        foreach (int i in players.Keys)
        {
            Rigidbody2D rig = players[i].GetComponent<Rigidbody2D>();

            state.ids[ind] = i;
            state.rigPos[ind] = rig.worldCenterOfMass;
            state.rigVel[ind] = rig.velocity;
            state.rigAngVel[ind] = rig.angularVelocity;
            state.rotations[ind] = rig.rotation;

            ind++;
        }

        return state;
    }

    private void ApplyInputPackage(GameObject obj, InputPkg pkg)
    {
        if (obj == null)
        {
            Debug.LogError("Attempting to apply input package to NULL OBJECT");
            return;
        }

        RobotScript robot = obj.GetComponent<RobotScript>();
        robot.Move(pkg.move, pkg.turn);

        //if (pkg.useWeapon)
        //    robot.Use();
    }

    private void FixedUpdate()
    {
        if (!gameRunning)
        {
            if (isServer && Input.GetKey(KeyCode.Space))
            {
                ServerStartGame();
            }
            return;
        }

        // Server: send state to everyone
        if (isServer)
        {
            frameOn++;

            RemoveDead();

            ProcessInputs();

            Physics2D.Simulate(Time.fixedDeltaTime);

            GameState s = GetCurrentState();

            // TODO: Do we need to send every frame? Probably...
            ClientReceiveState(s);
        }
        // Client: send input to server
        else
        {
            frameOn++;

            if (mPlayer != null)
            {
                PlayerScript ps = mPlayer.GetComponent<PlayerScript>();
                InputPkg pkg;
                pkg.move = ps.GetMove();
                pkg.turn = ps.GetTurn();
                //pkg.useWeapon = ps.useNextFrame;
                pkg.inputFrame = frameOn;

                if (ps.useNextFrame)
                    mPlayer.GetComponent<RobotScript>().LocalUse();

                pastInputs[frameOn] = pkg;

                int id = mPlayer.GetComponent<PlayerOnline>().myID;

                // TODO: Delete check
                if (id == -1)
                {
                    Debug.LogWarning("Game is running but I don't have my ID yet :(");
                }
                else
                {
                    ServerReceiveInput(id, pkg);
                    ApplyInputPackage(mPlayer, pkg);
                }
            }

            Physics2D.Simulate(Time.fixedDeltaTime);

            GameState s = GetCurrentState();
            pastGameStates.Enqueue(s);

            CheckPastState();
            waitingState = null;
        }
    }
}
