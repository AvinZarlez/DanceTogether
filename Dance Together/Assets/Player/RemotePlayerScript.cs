using UnityEngine;
using System.Collections;

public class RemotePlayerScript : MonoBehaviour {
    public int position;

    //GameObject localPlayer;
    Vector3 localPlayerStartingPosition;

    void Start()
    {
        GameObject localPlayer = GameObject.Find("LOCAL Player");
        localPlayerStartingPosition = localPlayer.GetComponent<LocalPlayerScript>().startingLocation;
    }

    void Update()
    {
        float step = Time.deltaTime; //Add a speed?
        Vector3 pos = localPlayerStartingPosition;
        pos.x += position;
        transform.position = Vector3.MoveTowards(transform.position, pos, step);
    }
}
