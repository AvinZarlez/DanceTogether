using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameListenerScript : CaptainsMessListener
{
	public enum NetworkState
	{
		Init,
		Offline,
		Connecting,
		Connected,
		Disrupted
	};
    [HideInInspector]
    public NetworkState networkState = NetworkState.Init;
	public Text networkStateField;
    //public ExampleGameSession gameSession;

    public GameObject gameManagerPrefab;

    public void Start()
    {
        ClientScene.RegisterPrefab(gameManagerPrefab);

        networkState = NetworkState.Offline;

        //networkStateField = GameObject.Find("UI_NetworkStateField").GetComponent<Text>();
    }

    public override void OnServerCreated()
    {
        GameManagerScript oldgameManager = FindObjectOfType<GameManagerScript>();
        if (oldgameManager == null)
        {
            GameObject gameManager = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gameManager);
        }
        else
        {
            Debug.LogError("GameManager already exists!");
        }
    }

    public override void OnStartConnecting()
	{
		networkState = NetworkState.Connecting;
	}

	public override void OnStopConnecting()
	{
		networkState = NetworkState.Offline;
	}

	public override void OnJoinedLobby()
	{
		networkState = NetworkState.Connected;

		//gameSession.OnJoinedLobby();
	}

	public override void OnLeftLobby()
	{
		networkState = NetworkState.Offline;

		//gameSession.OnLeftLobby();
	}

	public override void OnCountdownStarted()
	{
        //gameSession.OnCountdownStarted();

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager != null)
        {
            gameManager.CmdRotatePlayers(true);
        }
    }

	public override void OnCountdownCancelled()
	{
        //gameSession.OnCountdownCancelled();

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager != null)
        {
            gameManager.CmdRotatePlayers(false);
        }
    }

	public override void OnStartGame(List<CaptainsMessPlayer> aStartingPlayers)
	{
		Debug.Log("GO!");
        //gameSession.OnStartGame(aStartingPlayers);

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager != null)
        {
            gameManager.CmdStartMainCountdown();
        }
    }

	public override void OnAbortGame()
	{
		Debug.Log("ABORT!");
        //gameSession.OnAbortGame();

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager != null)
        {
            gameManager.CmdEndGame();
        }
    }

	void Update()
	{
		networkStateField.text = networkState.ToString();	
	}
}
