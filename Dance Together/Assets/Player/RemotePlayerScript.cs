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

        Vector3 goal = Vector3.zero;
        int col = position % 2;
        if (col == 1) goal.x = -100;
        else goal.x = 100;

        switch (position)
        {
            case 1:
            case 2:
                goal.y = 60;
                break;
            case 3:
            case 4:
                goal.y = -60;
                break;
            case 5:
            case 6:
                goal.y = 180;
                break;
            case 7:
            case 8:
                goal.y = -180;
                break;
            default:
                goal.y = -300;
                break;

        }

        goal.z -= 1;

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
