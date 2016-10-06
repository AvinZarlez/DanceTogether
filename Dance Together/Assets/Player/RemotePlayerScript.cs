using UnityEngine;
using DG.Tweening;
using System.Collections;

public class RemotePlayerScript : MonoBehaviour {

    #if UNITY_EDITOR
    //Debugging Varibles
    public bool debug_SortPlayers;
    #endif

    public int position;
    public int numberOfPlayers;

    public Vector3 SetPosition(int p, int numPlayers)
    {
        // Set our position, starts at 1 and goes up to (and including) numPlayers
        position = p;
        numberOfPlayers = numPlayers;

        return GetPosition();
    }


    public Vector3 GetPosition()
    {
        Vector3 goal = Vector3.zero;

        goal.y = -20;
        goal.x = 160 * (position);

        return goal;
    }
    
    public void Update()
    {
        #if UNITY_EDITOR
        if (debug_SortPlayers)
        {
            debug_SortPlayers = false;
            GetComponent<NetworkedPlayerScript>().playerButton.transform.DOLocalMove(SetPosition(position, numberOfPlayers), 0.5f);
        }
        #endif
    }
}
