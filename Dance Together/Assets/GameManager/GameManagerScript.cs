using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GameManagerScript : NetworkBehaviour
{
    // Count down timer for game start. Public so other scripts can monitor.
    [SyncVar, HideInInspector]
    public float countDown;

    [SyncVar, HideInInspector]
    public float endgameCountDown;

    [HideInInspector]
    static public GameManagerScript instance = null;

    [SyncVar]
    private byte currentGameState;
    // 200+ means the game is running

    private const float gameLength = 60; // How long the game lasts, in seconds.
    private const float endGameLength = 20; // How long the game lasts, in seconds.
    private const int songType = 1; // Hard coded song type. To make choice available later.

    private NetworkedPlayerScript networkedPScript;

    private int roundCount;

    [Client]
    public override void OnStartClient()
    {
        if (instance)
        {
            Debug.LogError("ERROR: Another Client!");
        }
        instance = this;
    }

    void Start()
    {
        countDown = -1;
        endgameCountDown = -1;
        roundCount = 0;
        GUIManagerScript.SetEndGameScreen(false);

        Random.InitState((int)System.Environment.TickCount);
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
        else if (endgameCountDown > 0)
        {
            endgameCountDown -= Time.deltaTime;

            if (endgameCountDown <= 0)
            {
                endgameCountDown = -1;
                if (networkedPScript == null)
                {
                    SetNPS();
                }
                networkedPScript.CmdReplayGame();
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

    public int GetRoundCount()
    {
        return roundCount;
    }

    private void SetNPS()
    {
        GameObject player = GameObject.Find("LOCAL Player");
        networkedPScript = player.GetComponent<NetworkedPlayerScript>();
    }

    [Command]
    public void CmdStartMainCountdown()
    {
        RpcStartMainCountdown();

        if (networkedPScript == null)
        {
            SetNPS();
        }
        networkedPScript.CmdStartGame();
    }

    [ClientRpc]
    void RpcStartMainCountdown()
    {
        roundCount++;
        currentGameState = 200;
        countDown = gameLength;
        GUIManagerScript.SetEndGameScreen(false);
    }

    [Command]
    public void CmdEndGame()
    {
        if (!IsInPostGame())
        {
            RpcEndGame();
            if (networkedPScript == null)
            {
                SetNPS();
            }
            networkedPScript.CmdEndGame(); //Let's stop this party
        }
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        countDown = 0;
        endgameCountDown = endGameLength;
        currentGameState = 210;
        GUIManagerScript.SetEndGameScreen(true);
    }

    [Command]
    public void CmdReplayGame()
    {
        RpcReplayGame();
    }

    [ClientRpc]
    public void RpcReplayGame()
    {
        endgameCountDown = -1;
        currentGameState = 0;
        GUIManagerScript.SetEndGameScreen(false);

        AudioManagerScript.instance.StartMenuMusic();
    }


    [Command]
    public void CmdCountdown(bool l)
    {
        RpcCountdown(l);
    }

    [ClientRpc]
    public void RpcCountdown(bool l)
    {
        GUIManagerScript.SetEndGameScreen(false);
        if (l)
        {
            //playerParent.GetComponent<PlayerParentScript>().LockAndSpin();
            AudioManagerScript.instance.PrepareGameMusic();
            AudioManagerScript.instance.PlayCountdown();
            GUIManagerScript.SetRulesButton(false);
            GUIManagerScript.SetPregameParent(false);
        }
        else {
            //playerParent.GetComponent<PlayerParentScript>().Unlock();
            Debug.Log("Countdown stopped!");
            AudioManagerScript.instance.StopSFX();
            AudioManagerScript.instance.StartMenuMusic();
            GUIManagerScript.SetRulesButton(true);
            GUIManagerScript.SetPregameParent(true);
            
            if (networkedPScript == null)
            {
                SetNPS();
            }
        }
    }

    [Command]
    public void CmdResetColor()
    {
        if (networkedPScript == null)
        {
            SetNPS();
        }
        networkedPScript.CmdSetColor();
    }
    
    public void ResetScore()
    {
        Debug.Log("GM ResetScore");
        if (networkedPScript == null)
        {
            SetNPS();
        }
        networkedPScript.ResetScore();
    }

    public int GetSongType()
    {
        return songType;
    }
}