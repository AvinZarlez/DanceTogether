using UnityEngine;
using UnityEngine.Networking;

public class GameManagerScript : NetworkBehaviour
{
    // Count down timer for game start. Public so other scripts can monitor.
    [SyncVar, HideInInspector]
    public float countDown;
    
    static public GameManagerScript instance = null;

    [SyncVar]
    private byte currentGameState;
    // 200+ means the game is running

    private const float gameLength = 30; // How long the game lasts, in seconds.
    
    private NetworkedPlayerScript networkedPScript; //Public so it can set itself

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

            if (GameManagerScript.instance.IsInMainGameplay())
            {
                if (countDown <= 0)
                {
                    CmdEndGame();
                    networkedPScript.CmdEndGame(); //Let's stop this party
                }
            }
            else
            {
                if (countDown <= 0)
                {
                    CmdStartMainCountdown(); //Let's get this party started;
                }
            }
        }
    }

    public bool IsGameStarted()
    {
        return (currentGameState >= 200);
    }

    public bool IsInMainGameplay()
    {
        return (currentGameState > 200);
    }

    public bool IsInPostGame()
    {
        return (currentGameState >= 210);
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
    public void CmdStartGame()
    {
        RpcStartGame();
    }

    [ClientRpc]
    void RpcStartGame()
    {
        currentGameState = 200;
        countDown = 5f;
    }

    [Command]
    public void CmdStartMainCountdown()
    {
        RpcStartMainCountdown();
    }

    [ClientRpc]
    void RpcStartMainCountdown()
    {
        currentGameState = 201;
        countDown = gameLength;
    }

    [Command]
    public void CmdEndGame()
    {
        RpcEndGame();
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        currentGameState = 210;
    }

}