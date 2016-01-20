using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class NetworkedPlayerScript : NetworkBehaviour
{
    [SerializeField]
    private int numberOfSongs; //Temp? Better way to load songs than number, some kind of list?

    [SerializeField]
    private LocalPlayerScript localPScript;
    [SerializeField]
    private RemotePlayerScript remotePScript;
    
    //public Camera mainCamera; //Not sure if I need to mess with camera?

    // Count down timer for game start. Public so other scripts can monitor.
    [SyncVar]
    public float countDown;

    [SyncVar]
    private int songID;

    [SyncVar]
    private Color color;

    [SyncVar]
    private bool isGameStarted;

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

    public override void OnStartLocalPlayer()
    {
        gameObject.name = "LOCAL Player";

        //mainCamera.enabled = true; //Not sure if I need to mess with camera?
        remotePScript.enabled = false;
        localPScript.enabled = true;

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
        return isGameStarted;
    }

    public Color GetColor()
    {
        return color;
    }

    public int GetSongID()
    {
        return songID;
    }

    [Command]
    public void CmdStartGame()
    {
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");

        int length = players.Length;

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
        isGameStarted = true;
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
        isGameStarted = false;
    }

}