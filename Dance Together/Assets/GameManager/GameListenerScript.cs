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

    public GameObject disconnectButton;
    public GameObject menuParent;
    public GameObject mainViewParent;

    public GameObject gameManagerPrefab;

    public void Start()
    {
        ClientScene.RegisterPrefab(gameManagerPrefab);

        networkState = NetworkState.Offline;

        //networkStateField = GameObject.Find("UI_NetworkStateField").GetComponent<Text>();

        mainViewParent.SetActive(false);
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
        mainViewParent.SetActive(true);
        GUIManagerScript.HideMainView();

        networkState = NetworkState.Connecting;

        disconnectButton.SetActive(true);
        menuParent.SetActive(false);
    }

	public override void OnStopConnecting()
    {
        mainViewParent.SetActive(false);

        networkState = NetworkState.Offline;

        disconnectButton.SetActive(false);
        menuParent.SetActive(true);

        GUIManagerScript.SetBGColor(new Color32(32, 32, 32, 255));
    }

	public override void OnJoinedLobby()
    {
        mainViewParent.SetActive(true);
        GUIManagerScript.HideMainView();

        networkState = NetworkState.Connected;

        //gameSession.OnJoinedLobby();

        disconnectButton.SetActive(true);
        menuParent.SetActive(false);
    }

	public override void OnLeftLobby()
    {
        mainViewParent.SetActive(false);

        networkState = NetworkState.Offline;

        //gameSession.OnLeftLobby();

        disconnectButton.SetActive(false);
        menuParent.SetActive(true);

        GUIManagerScript.SetBGColor(new Color32(32, 32, 32, 255));
    }

	public override void OnCountdownStarted()
	{
        //gameSession.OnCountdownStarted();

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager != null)
        {
            gameManager.CmdCountdown(true);
        }
    }

	public override void OnCountdownCancelled()
	{
        //gameSession.OnCountdownCancelled();

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        if (gameManager != null)
        {
            gameManager.CmdCountdown(false);
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
		networkStateField.text = "Status: " + networkState.ToString();	
	}
}
