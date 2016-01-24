using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManagerScript : NetworkBehaviour
{
    // Count down timer for game start. Public so other scripts can monitor.
    [SyncVar, HideInInspector]
    public float countDown;

    [SyncVar]
    private byte currentGameState;
    // 200+ means the game is running

    private const float gameLength = 30; // How long the game lasts, in seconds.

    private Text countdownText; // UI text object named "UI_Countdown"

    [System.NonSerialized]
    public NetworkedPlayerScript networkedPScript;

    void Start()
    {
        countDown = -1;

        GameObject obj = GameObject.Find("UI_Countdown");
        countdownText = obj.GetComponent<Text>();
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

    void Update()
    {
        if (IsInPostGame())
        {
            countdownText.text = "GAME OVER!" + " " + networkedPScript.GetSongID();
        }
        else if (countDown > 0)
        {

            countDown -= Time.deltaTime;

            if (IsInMainGameplay())
            {
                if (countDown <= 0)
                {
                    CmdEndGame();
                    networkedPScript.CmdEndGame(); //Let's stop this party
                }
                else
                {
                    countdownText.text = "" + Mathf.Ceil(countDown);
                }
            }
            else
            {
                if (countDown <= 0)
                {
                    countdownText.text = "";
                    CmdStartMainCountdown(); //Let's get this party started;
                }
                else if (countDown < 1)
                {
                    countdownText.text = "DANCE!";
                }
                else if (countDown >= (4f)) //Plus one second for the "Dance" end 
                {
                    countdownText.text = "Ready?";
                }
                else
                {
                    countdownText.text = "" + Mathf.Floor(countDown);
                }
            }
        }
    }
}