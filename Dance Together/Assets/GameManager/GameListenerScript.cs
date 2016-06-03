﻿using System;
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

    public GameObject disconnectButton;
    public GameObject menuParent;

    public GameObject gameManagerPrefab;

    public void Start()
    {
        ClientScene.RegisterPrefab(gameManagerPrefab);

        networkState = NetworkState.Offline;

        //networkStateField = GameObject.Find("UI_NetworkStateField").GetComponent<Text>();

        disconnectButton.SetActive(false);
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

        disconnectButton.SetActive(true);
        menuParent.SetActive(false);
    }

	public override void OnStopConnecting()
	{
		networkState = NetworkState.Offline;

        disconnectButton.SetActive(false);
        menuParent.SetActive(true);

        Camera.main.backgroundColor = new Color32(32, 32, 32, 255);

    }

	public override void OnJoinedLobby()
	{
		networkState = NetworkState.Connected;

        //gameSession.OnJoinedLobby();

        disconnectButton.SetActive(true);
        menuParent.SetActive(false);

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.SFXClips.DanceTogether);
    }

	public override void OnLeftLobby()
	{
		networkState = NetworkState.Offline;

        //gameSession.OnLeftLobby();

        disconnectButton.SetActive(false);
        menuParent.SetActive(true);

        Camera.main.backgroundColor = new Color32(32, 32, 32,255);
    }

	public override void OnCountdownStarted()
	{
        //gameSession.OnCountdownStarted();

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager != null)
        {
            gameManager.CmdRotatePlayers(true);
        }

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.SFXClips.Countdown);
    }

	public override void OnCountdownCancelled()
	{
        //gameSession.OnCountdownCancelled();

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager != null)
        {
            gameManager.CmdRotatePlayers(false);
        }

        AudioManagerScript.instance.StopSFX();
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

        AudioManagerScript.instance.PlayCountdown();
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
		networkStateField.text = "Status: " + networkState.ToString();	
	}
}
