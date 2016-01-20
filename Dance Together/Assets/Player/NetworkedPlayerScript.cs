using UnityEngine;
using UnityEngine.Networking;

public class NetworkedPlayerScript : NetworkBehaviour
{
    [SerializeField]
    private LocalPlayerScript localPScript;
    [SerializeField]
    private RemotePlayerScript remotePScript;
    //public Camera mainCamera; //Not sure if I need to mess with camera?

    [SyncVar,HideInInspector]
    public int songID;

    [SyncVar]
    public Color color;

    [SyncVar,HideInInspector]
    public float countDown;

    [SyncVar]
    public bool isGameStarted;

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

    public bool GetIsGameStarted()
    {
        return isGameStarted;
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

    [Command]
    public void CmdStartGame()
    {
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<NetworkedPlayerScript>().RpcStartGame();
        }
    }

    [ClientRpc]
    public void RpcStartGame()
    {
        isGameStarted = true;
    }

}