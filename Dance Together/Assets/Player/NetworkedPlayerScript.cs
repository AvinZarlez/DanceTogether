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

    // Am I ready to start the game?
    //[SyncVar, HideInInspector]
    //public bool playerReady;

    [SyncVar]
    private int songID;
    
    [SyncVar]
    private int matchSongID; // The other player this player has picked as a match

    [SyncVar]
    private int color = -1;

    // To make referencing easier/less calls.
    private Light playerLight;


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
        playerReady = ready;

        if (AreAllPlayersReady())
        {
            GUIManagerScript.SetButtonInteractable(true);
        }
        else
        {
            GUIManagerScript.SetButtonInteractable(false);
        }
    }

    void SortPlayers()
    {
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");
        int i = 0;
        int size = players.Length;

        foreach (GameObject player in players)
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

    /*bool AreAllPlayersReady()
    {
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");

        bool allPlayersReady = true;
        foreach (GameObject player in players)
        {
            if (!player.GetComponent<NetworkedPlayerScript>().playerReady)
            {
                allPlayersReady = false;
                break;
            }
        }

        return allPlayersReady; // TODO : Check if there are four players
    }*/

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
        if (playerReady)
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

        playerReady = false;

        CmdSetColor();

        base.OnStartLocalPlayer();
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        // Brief delay to let SyncVars propagate
        Invoke("SortPlayers", 0.5f);
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

        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
            playerColors.Remove(nps.GetColor());
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
        SetReady(!playerReady);
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
                CmdMainButtonPressed();
            }
        }
    }
        
    [ClientRpc]
    public void RpcReplyGame()
    {
        GUIManagerScript.SetReplyButton(false);
    }

    [Command]
    public void CmdMainButtonPressed()
    {
            if (AreAllPlayersReady()) //Redundant?
            {
                GameManagerScript.instance.CmdStartGame();

                GameObject[] players;
                players = GameObject.FindGameObjectsWithTag("Player");

                int length = players.Length;

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

                foreach (GameObject player in players)
                {
                    //Recycle local J variable, don't care about last value
                    j = Random.Range(0, playerSongChoice.Count);

                    //Tell the player which song they got
                    NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
                    nps.RpcStartGame(playerSongChoice[j]);

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
        if (!playerReady)
        {
            Assert.IsFalse(playerReady, "Player wasn't ready, but the server thought they were!");
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
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
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