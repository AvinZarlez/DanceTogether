using UnityEngine;
using UnityEngine.Networking;

public class NetworkedPlayerScript : NetworkBehaviour
{
    public LocalPlayerScript localPScript;
    public RemotePlayerScript remotePScript;
    //public Camera mainCamera; //Not sure if I need to mess with camera?

    [SyncVar]
    public int playerNum = 0;

    public override void OnStartLocalPlayer()
    {
        //mainCamera.enabled = true; //Not sure if I need to mess with camera?
        localPScript.enabled = true;
        remotePScript.enabled = false;

        gameObject.name = "LOCAL Player";

        base.OnStartLocalPlayer();
    }

    public override void OnStartClient()
    {
        SortPlayers();
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        //SortPlayers();
        base.OnStartServer();
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
        }
    }
}