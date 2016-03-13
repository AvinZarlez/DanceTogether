using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class NetworkedPlayerScript : CaptainsMessPlayer
{
    private const float playerLightRange = 2f;

    private float playerRangeMultiplier = 1;

    [SerializeField]
    public LocalPlayerScript localPScript; //TEMP made public for checking in sort players
    [HideInInspector]
    public RemotePlayerScript remotePScript;

    private GameObject playerParent;

    [SyncVar]
    private int songID;

    [SyncVar]
    private int matchSongID; // The other player this player has picked as a match

    [SyncVar]
    private int color = -1;

    [SyncVar, HideInInspector]
    public float captainsCountdown = 0;

    // To make referencing easier/less calls.
    private Light playerLight;

    [HideInInspector]
    public CaptainsMess mess;
    public void Awake()
    {
        mess = FindObjectOfType(typeof(CaptainsMess)) as CaptainsMess;
    }

    void SetColor()
    {
        if (color != -1)
        {
            Color c = ColorScript.GetColor(color);
            GetComponentInChildren<Renderer>().material.color = c;
            GetComponentInChildren<Light>().color = c;
        }
    }

    bool AreAllPlayersMatched()
    {
        List<CaptainsMessPlayer> players = mess.Players();

        bool allPlayersMatched = true;
        foreach (CaptainsMessPlayer player in players)
        {
            if (player.GetComponent<NetworkedPlayerScript>().GetMatchSongID() == -1)
            {
                allPlayersMatched = false;
                break;
            }
        }

        return allPlayersMatched;
    }

    void SetReady(bool ready)
    {
        if (ready)
        {
            SendReadyToBeginMessage();

            GUIManagerScript.SetButtonText("Cancel");
        }
        else
        {
            SendNotReadyToBeginMessage();
            GUIManagerScript.SetButtonText("Dance");
        }
    }

    void SortPlayers()
    {
        List<CaptainsMessPlayer> players = mess.Players();
        int i = 0;
        int size = players.Count;

        foreach (CaptainsMessPlayer player in players)
        {
            if (player.name != "LOCAL Player")
            {
                player.GetComponent<RemotePlayerScript>().SetPosition(++i, size);
            }
            player.GetComponent<NetworkedPlayerScript>().SetColor();
        }

        if (size >= 4)
        {
            GUIManagerScript.SetButton(true);
        }
        else
        {
            GUIManagerScript.SetButton(false);
        }
    }

    void Start()
    {
        playerLight = GetComponentInChildren<Light>();
        playerParent = GameObject.FindWithTag("PlayerParent");

        if (isLocalPlayer)
        {
            playerRangeMultiplier = 1.5f;
        }
        else
        {
            transform.parent = playerParent.transform;
            transform.localPosition = Vector3.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "LOCAL Player" && GameManagerScript.instance.IsGameStarted()) //Temp! Change to singleton?
        {
            remotePScript.growing = true;
            playerRangeMultiplier = 1.5f;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isLocalPlayer)
        {
            remotePScript.growing = false;
            playerRangeMultiplier = 1f;
        }
    }

    public override void Update()
    {
        base.Update();

        // Grow as player overlaps
        if (readyToBegin)
        {
            if (playerLight.range < (playerLightRange * playerRangeMultiplier))
            {
                playerLight.range += 0.1f;
            }
            if (playerLight.range > (playerLightRange * playerRangeMultiplier))
            {
                playerLight.range -= 0.1f;
            }
        }
        else
        {
            if (playerLight.range > 0)
            {
                playerLight.range -= 0.1f;
            }
        }

        if (isServer)
            captainsCountdown = mess.CountdownTimer();
    }

    public override void OnStartLocalPlayer()
    {
        gameObject.name = "LOCAL Player";

        GameManagerScript.instance.CmdSetNPS();

        remotePScript.enabled = false;
        localPScript.enabled = true;

        CmdSetColor();

        base.OnStartLocalPlayer();
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        // Brief delay to let SyncVars propagate
        Invoke("SortPlayers", 0.5f);
    }

    public List<CaptainsMessPlayer> GetPlayers()
    {
        return mess.Players();
    }

    public int GetColor()
    {
        return color;
    }

    public int GetSongID()
    {
        return songID;
    }

    public int GetMatchSongID()
    {
        return matchSongID;
    }

    [Command]
    void CmdSetColor()
    {
        List<int> playerColors = new List<int>();

        int length = ColorScript.colors.Length;
        for (int i = 0; i < length; i++)
        {
            playerColors.Add(i);
        }

        List<CaptainsMessPlayer> players = GetPlayers();
        foreach (CaptainsMessPlayer player in players)
        {
            playerColors.Remove(player.GetComponent<NetworkedPlayerScript>().GetColor());
        }

        RpcSetColor(playerColors[0]);   //<- new, always get first way. Old random way: Random.Range(0, playerColors.Count)]);
    }

    [ClientRpc]
    void RpcSetColor(int c)
    {
        color = c;
        SetColor();

        if (isLocalPlayer)
        {
            Color clr = ColorScript.GetColor(c);
            clr = clr * 0.25f;
            Camera.main.backgroundColor = clr;
        }
    }

    public void ToggleReady()
    {
        SetReady(!readyToBegin);
    }

    public void MainButtonPressed()
    {
        if (GameManagerScript.instance.IsInPostGame())
        {
            CmdReplayGame();
        }
        else
        {
            if (localPScript.WasMatchedPressed())
            {
                matchSongID = localPScript.choiceSongID;
                Assert.AreNotEqual<int>(-1, matchSongID, "No player was matched");
                GUIManagerScript.SetMatchButton(false);

                if (AreAllPlayersMatched())
                {
                    //Every player is matched, end the game early.
                    GameManagerScript.instance.CmdEndGame();
                }
            }
            else
            {
                ToggleReady();
            }
        }
    }

    [Command]
    public void CmdReplayGame()
    {
        GameManagerScript.instance.CmdReplayGame();

        List<CaptainsMessPlayer> players = GetPlayers();
        foreach (CaptainsMessPlayer player in players)
        {
            player.GetComponent<NetworkedPlayerScript>().RpcReplayGame();
        }
    }
    [ClientRpc]
    public void RpcReplayGame()
    {
        GUIManagerScript.SetReplayButton(false);
    }

    [Command]
    public void CmdStartGame()
    {
        if (mess.AreAllPlayersReady()) //Redundant?
        {

            List<CaptainsMessPlayer> players = GetPlayers();

            int length = players.Count;

            Assert.IsTrue(length >= 4, "There must be >=4 players!");

            int numSongsToPick = (length / 2);

            List<int> songs = new List<int>(); //List of the songID's we'll use this game.

            int numberOfSongs = AudioManagerScript.instance.GetNumSongs();
            for (int i = 0; i < numSongsToPick; i++)
            {
                int rand;
                do
                {
                    rand = Random.Range(0, numberOfSongs);
                }
                while (songs.Contains(rand));
                songs.Add(rand);
            }

            List<int> playerSongChoice = new List<int>(); // Final list will pull from

            // Remember, final list will have at least 2 of every choice!
            int j = 0;
            if (length % 2 != 0) { j = -1; }
            for (int k = 0; k < length; k++)
            {
                if (j == -1)
                {
                    playerSongChoice.Add(songs[Random.Range(0, numSongsToPick)]);
                }
                else
                {
                    playerSongChoice.Add(songs[(int)(j / 2)]);
                }
                j++;
            }

            foreach (CaptainsMessPlayer player in players)
            {
                //Recycle local J variable, don't care about last value
                j = Random.Range(0, playerSongChoice.Count);

                //Tell the player which song they got
                player.GetComponent<NetworkedPlayerScript>().RpcStartGame(playerSongChoice[j]);

                //Remove that entry from list. 
                playerSongChoice.RemoveAt(j);
            }

        } //Close if statement for checking if all players ready
    }
    [ClientRpc]
    public void RpcStartGame(int s)
    {
        songID = s;

        playerParent.GetComponent<PlayerParentScript>().Unlock();

        // Bullshit code. Temp? Maybe not?
        // What if player WAS ready, but now that we're actually starting they are no longer?
        // Too late for them! Let's double check
        if (!readyToBegin)
        {
            Assert.IsFalse(readyToBegin, "Player wasn't ready, but the server thought they were!");
            // Oh noes! What do we do? Let's cheat:
            SetReady(true);
            // See buddy, you were ready the whole time, right?
        }
        matchSongID = -1;
        GUIManagerScript.SetButton(false);

        AudioManagerScript.instance.StartGameMusic();

        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    [Command]
    public void CmdEndGame()
    {
        List<CaptainsMessPlayer> players = GetPlayers();
        foreach (CaptainsMessPlayer player in players)
        {
            player.GetComponent<NetworkedPlayerScript>().RpcEndGame();
        }
    }
    [ClientRpc]
    public void RpcEndGame()
    {
        SetReady(false);

        if (localPScript.WasMatchedPressed())
        {
            localPScript.BackButtonPressed();
        }

        GUIManagerScript.SetReplayButton(true);

        AudioManagerScript.instance.EndGameMusic();

        // Enable screen dimming
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    [ClientRpc]
    public void RpcRotatePlayers(bool l)
    {
        if (l)
        {
            playerParent.GetComponent<PlayerParentScript>().LockAndSpin();
            AudioManagerScript.instance.PrepareGameMusic();
        }
        else {
            playerParent.GetComponent<PlayerParentScript>().Unlock();
        }
    }
}