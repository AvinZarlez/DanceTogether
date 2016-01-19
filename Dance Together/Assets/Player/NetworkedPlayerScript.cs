using UnityEngine;
using UnityEngine.Networking;

public class NetworkedPlayerScript : NetworkBehaviour {
    public LocalPlayerScript localPScript;
    //public Camera mainCamera; //Not sure if I need to mess with camera?

    public override void OnStartLocalPlayer()
    {
        //mainCamera.enabled = true; //Not sure if I need to mess with camera?
        localPScript.enabled = true;

        gameObject.name = "LOCAL Player";
        base.OnStartLocalPlayer();
    }
}
