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

    private const float gameLength = 60; // How long the game lasts, in seconds.
    
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
        roundCount = 0;
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
    public void CmdCountdown(bool l)
    {
        RpcCountdown(l);
    }

    [ClientRpc]
    public void RpcCountdown(bool l)
    {
        if (l)
        {
            //playerParent.GetComponent<PlayerParentScript>().LockAndSpin();
            AudioManagerScript.instance.PrepareGameMusic();
            AudioManagerScript.instance.PlayCountdown();
            GUIManagerScript.SetRulesButton(false);
        }
        else {
            //playerParent.GetComponent<PlayerParentScript>().Unlock();
            Debug.Log("Countdown stopped!");
            AudioManagerScript.instance.StopSFX();
            AudioManagerScript.instance.StartMenuMusic();
            GUIManagerScript.SetRulesButton(true);
        }
    }
}