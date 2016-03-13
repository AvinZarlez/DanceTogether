using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GameManagerScript : NetworkBehaviour
{
    // Count down timer for game start. Public so other scripts can monitor.
    [SyncVar, HideInInspector]
    public float countDown;

    [HideInInspector]
    static public GameManagerScript instance = null;

    [SyncVar]
    private byte currentGameState;
    // 200+ means the game is running

    private const float gameLength = 90; // How long the game lasts, in seconds.
    
    private NetworkedPlayerScript networkedPScript;

    [SerializeField]
    private GameObject playerParent;

    [SyncVar]
    private List<LocalPlayerScript> matchedPlayers; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        countDown = -1;
    }

    void Update()
    {
        if (countDown > 0)
        {
            countDown -= Time.deltaTime;
            
                if (countDown <= 0)
                {
                    CmdEndGame();
                }
        }
    }

    public bool IsGameStarted()
    {
        return (currentGameState >= 200);
    }

    public bool IsInPostGame()
    {
        return (currentGameState >= 210);
    }

    [Command]
    public void CmdSetPlace(LocalPlayerScript player)
    {
        RpcSetPlace(player);
    }

    [ClientRpc]
    public void RpcSetPlace(LocalPlayerScript player)
    {
        matchedPlayers.Add(player);

        player.SetPlace(matchedPlayers.Count);
    }

    [Command]
    public void CmdSetNPS()
    {
        RpcSetNPS();
    }

    [ClientRpc]
    void RpcSetNPS()
    {
        GameObject player = GameObject.Find("LOCAL Player");
        networkedPScript = player.GetComponent<NetworkedPlayerScript>();
    }

    [Command]
    public void CmdStartMainCountdown()
    {
        RpcStartMainCountdown();

        networkedPScript.CmdStartGame();

        matchedPlayers = new List<LocalPlayerScript>();
    }

    [ClientRpc]
    void RpcStartMainCountdown()
    {
        currentGameState = 200;
        countDown = gameLength;
    }

    [Command]
    public void CmdEndGame()
    {
        RpcEndGame();
        networkedPScript.CmdEndGame(); //Let's stop this party
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        countDown = 0;
        currentGameState = 210;
    }

    [Command]
    public void CmdReplayGame()
    {
        RpcReplayGame();
    }

    [ClientRpc]
    public void RpcReplayGame()
    {
        currentGameState = 0;

        AudioManagerScript.instance.StartMenuMusic();
    }


    [Command]
    public void CmdRotatePlayers(bool l)
    {
        List<CaptainsMessPlayer> players = networkedPScript.GetPlayers();
        foreach (CaptainsMessPlayer player in players)
        {
            player.GetComponent<NetworkedPlayerScript>().RpcRotatePlayers(l);
        }
    }
}