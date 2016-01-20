using UnityEngine;
using UnityEngine.Networking;

public class NetworkedPlayerScript : NetworkBehaviour
{
    public LocalPlayerScript localPScript;
    public RemotePlayerScript remotePScript;
    //public Camera mainCamera; //Not sure if I need to mess with camera?

    [SyncVar]
    public Color color;

    [SyncVar]
    public float countDown;

    public override void OnStartLocalPlayer()
    {
        //mainCamera.enabled = true; //Not sure if I need to mess with camera?
        localPScript.enabled = true;
        remotePScript.enabled = false;

        gameObject.name = "LOCAL Player";

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
}