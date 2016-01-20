using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class NetworkedPlayerScript : NetworkBehaviour
{
    private const float countDownTimerStartValue = 3f; //Three second start

    [SerializeField]
    private int numberOfSongs; //Temp? Better way to load songs than number, some kind of list?

    [SerializeField]
    private LocalPlayerScript localPScript;
    [SerializeField]
    private RemotePlayerScript remotePScript;

    [SerializeField]
    private GameObject playerParent;
    
    //public Camera mainCamera; //Not sure if I need to mess with camera?

    // Count down timer for game start. Public so other scripts can monitor.
    [SyncVar,HideInInspector]
    public float countDown;

    // Am I ready to start the game?
    [SyncVar, HideInInspector]
    public bool playerReady;

    [SyncVar]
    private int songID;

    [SyncVar]
    private Color color;

    [SyncVar]
    private byte currentGameState;
    // 200+ means the game is running

    // To make referencing easier/less calls.
    private Light playerLight;

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

    void SetColor()
    {
        GetComponent<Renderer>().material.color = color;
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

    void Start ()
    {
        playerLight = GetComponentInChildren<Light>();
        if (!isLocalPlayer)
        {
            playerParent = GameObject.FindWithTag("PlayerParent");
            transform.parent = playerParent.transform;
            transform.localPosition = Vector3.zero;
            Debug.Log("This ran");
        }
    }

    void ToggleLight(bool enable)
    {
        if (enable)
        {
            playerLight.color = color;
        }
        playerLight.enabled = enable;
    }

    public override void OnStartLocalPlayer()
    {
        gameObject.name = "LOCAL Player";

        //mainCamera.enabled = true; //Not sure if I need to mess with camera?
        remotePScript.enabled = false;
        localPScript.enabled = true;

        countDown = -1;
        playerReady = false;

        CmdSetColor(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));

        base.OnStartLocalPlayer();
    }

    public override void OnStartClient()
    {
        SortPlayers();
        base.OnStartClient();
    }

    public bool GetIsGameStarted()
    {
        return (currentGameState >= 200);
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
    public void CmdToggleReady(bool ready)
    {
        RpcToggleReady(ready);
    }
    [ClientRpc]
    public void RpcToggleReady(bool ready)
    {
        playerReady = ready;
        ToggleLight(ready);
    }

    [Command]
    public void CmdStartGame()
    {
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");

        int length = players.Length;

        Assert.IsTrue(length > 1); //TEMP - Should be at least 3 or 4 players.

        int numSongsToPick = (length / 2);

        List<int> songs = new List<int>(); //List of the songID's we'll use this game.

        for (int i=0; i<numSongsToPick; i++)
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
        for (int k=0; k < length; k++)
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
    }
    [ClientRpc]
    public void RpcStartGame(int s)
    {
        songID = s;
        currentGameState = 200;

        countDown = countDownTimerStartValue + 0.999999999f;
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
        currentGameState = 0;
    }

}