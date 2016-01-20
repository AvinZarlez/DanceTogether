using UnityEngine;
using UnityEngine.Networking;

public class NetworkedPlayerScript : NetworkBehaviour
{
    public LocalPlayerScript localPScript;
    public RemotePlayerScript remotePScript;
    //public Camera mainCamera; //Not sure if I need to mess with camera?

    public int playerNum = 0;

    public override void OnStartLocalPlayer()
    {
        //mainCamera.enabled = true; //Not sure if I need to mess with camera?
        localPScript.enabled = true;
        remotePScript.enabled = false;

        gameObject.name = "LOCAL Player";
        
        base.OnStartLocalPlayer();
    }

    void Start()
    {
        if (!isLocalPlayer)
        {
            remotePScript.MovePlayer();
        }
    }
}