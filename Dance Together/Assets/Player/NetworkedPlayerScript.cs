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
    [SerializeField]
    private GameObject gameManagerPrefab;

    private GameObject playerParent;
    
    [SyncVar]
    private int songID;
    
    [SyncVar]
    private int matchSongID; // The other player this player has picked as a match

    [SyncVar]
    private int color = -1;

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
        if (color != -1) {
            Color c = ColorScript.GetColor(color);
            GetComponentInChildren<Renderer>().material.color = c;
            GetComponentInChildren<Light>().color = c;
        }
    }

    void SetReady(bool ready)
    {
        if (ready)
        {
            SendReadyToBeginMessage();

            if (mess.AreAllPlayersReady())
            {
                GUIManagerScript.SetButtonInteractable(true);
            }
            else
            {
                GUIManagerScript.SetButtonInteractable(false);
            }
        }
        else
        {
            SendNotReadyToBeginMessage();
            GUIManagerScript.SetButtonInteractable(false);
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
    }

    public override void OnStartLocalPlayer()
    {
        gameObject.name = "LOCAL Player";
        
        SetUpGMScript();

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

    //[Server]
    void SetUpGMScript()
    {
        GameObject gms;
        if (GameManagerScript.instance == null)
        {
            gms = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gms);
        }
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
            clr = clr*0.25f;
            Camera.main.backgroundColor = clr;
        }
    }

    [Command]
    public void CmdToggleReady()
    {
        RpcToggleReady();
    }
    [ClientRpc]
    public void RpcToggleReady()
    {
        SetReady(!readyToBegin);
    }

    public void MainButtonPressed()
    {
        if (GameManagerScript.instance.IsInPostGame())
        {
            GameManagerScript.instance.CmdReplyGame();
        }
        else
        {
            if (localPScript.WasMatchedPressed())
            {
                matchSongID = localPScript.choiceSongID;
                Assert.AreNotEqual<int>(-1, matchSongID, "No player was matched");
                GUIManagerScript.SetMatchButton(false);
            }
            else
            {
            }
        }
    }
        
    [ClientRpc]
    public void RpcReplyGame()
    {
        GUIManagerScript.SetReplyButton(false);
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

        playerParent.GetComponent<PlayerParentScript>().LockAndSpin();

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

        AudioManagerScript.instance.PrepareGameMusic();
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

        GUIManagerScript.SetReplyButton(true);

        AudioManagerScript.instance.EndGameMusic();
    }
}