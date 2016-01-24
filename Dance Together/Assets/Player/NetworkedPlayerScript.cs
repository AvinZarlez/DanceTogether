using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class NetworkedPlayerScript : NetworkBehaviour
{
    private const float playerLightRange = 2f;

    private float playerRangeMultiplier = 1;

    [SerializeField]
    private int numberOfSongs; //Temp? Better way to load songs than number, some kind of list?

    [SerializeField]
    private LocalPlayerScript localPScript;
    [SerializeField]
    private RemotePlayerScript remotePScript;

    private GameObject playerParent;

    // Am I ready to start the game?
    [SyncVar, HideInInspector]
    public bool playerReady;

    [SyncVar]
    private int songID;

    [SyncVar]
    private Color color;

    // To make referencing easier/less calls.
    private Light playerLight;
    private GameManagerScript GMScript;

    void SetColor()
    {
        GetComponentInChildren<Renderer>().material.color = color;
    }

    void SetReady(bool ready)
    {
        playerReady = ready;
        ToggleLight(playerReady);

        if (AreAllPlayersReady()) //TEMP! Add a start button?
        {
            CmdStartGame();
            GMScript.CmdStartGame();
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
    }

    void ToggleLight(bool enable)
    {
        if (enable)
        {
            playerLight.color = color;
        }
    }

    void Start ()
    {
        playerLight = GetComponentInChildren<Light>();
        playerParent = GameObject.FindWithTag("PlayerParent");

        GameObject gameManager = GameObject.FindWithTag("GameController");
        GMScript = gameManager.GetComponent<GameManagerScript>();

        if (isLocalPlayer)
        {
            playerRangeMultiplier = 1.5f;
            GMScript.networkedPScript = this;
        }
        else
        {
            transform.parent = playerParent.transform;
            transform.localPosition = Vector3.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "LOCAL Player" && GMScript.IsGameStarted() ) //Temp! Change to singleton?
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

    void FixedUpdate()
    {
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
        
        remotePScript.enabled = false;
        localPScript.enabled = true;

        playerReady = false;

        CmdSetColor(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));

        base.OnStartLocalPlayer();
    }

    public override void OnStartClient()
    {
        SortPlayers();
        base.OnStartClient();
    }

    /*public Color GetColor()
    {
        return color;
    }*/

    public int GetSongID()
    {
        return songID;
    }

    [Command]
    void CmdSetColor(Color c)
    {
        RpcSetColor(c);
    }

    [ClientRpc]
    void RpcSetColor(Color c)
    {
        color = c;
        SetColor();
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

    bool AreAllPlayersReady()
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
    }

    [Command]
    public void CmdStartGame()
    {
        if (AreAllPlayersReady()) //Redundant?
        {
            GameObject[] players;
            players = GameObject.FindGameObjectsWithTag("Player");
            
            int length = players.Length;

            Assert.IsTrue(length >= 4, "There must be >=4 players!");

            int numSongsToPick = (length / 2);

            List<int> songs = new List<int>(); //List of the songID's we'll use this game.

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
    }

}