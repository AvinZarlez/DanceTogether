using UnityEngine;
using System.Collections;

public class RemotePlayerScript : MonoBehaviour {
    
    public int position;
    public int numberOfPlayers;
    
    public Vector3 SetPosition(int p, int numPlayers)
    {
        // Set our position, starts at 1 and goes up to (and including) numPlayers
        position = p;
        numberOfPlayers = numPlayers;

        Vector3 goal = Vector3.zero;
        int col = position % 3;
        if (col == 1) goal.x = -160;
        else if (col == 2) goal.x = 160;

        if ((position >= 4)&&(position <= 6)) goal.y = -64;
        else if (position > 6) goal.y = 64;

        goal.z -= 1;

        return goal;
    }
}
