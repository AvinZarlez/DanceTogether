using UnityEngine;
using System.Collections;

public class RemotePlayerScript : MonoBehaviour {
    [SerializeField]
    int distance = 3;
    [SerializeField]
    int movementSpeed = 5;

    int position;
    int numberOfPlayers;

    void Start()
    {
        /*GameObject localPlayer = GameObject.Find("LOCAL Player");
        if (localPlayer)
        {
            transform.position = localPlayer.GetComponent<LocalPlayerScript>().startingLocation;
        }*/
    }

    void Update()
    {
        // Move towards desired location
        float step = movementSpeed * Time.deltaTime;
        Vector3 goal = Vector3.zero;
        float degreeMath;
        if (numberOfPlayers > 2) degreeMath = position * (2 * Mathf.PI) / (numberOfPlayers - 1);
        else degreeMath = Mathf.PI; //Cheat if 2 players. If theory this won't happen if only one player, but just in case also prevents divide by 0.
        goal.x += distance * Mathf.Cos(degreeMath);
        goal.y += distance * Mathf.Sin(degreeMath);
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, goal, step);
    }

    public void SetPosition(int p, int numPlayers)
    {
        // Set our position, starts at 1 and goes up to (and including) numPlayers
        position = p;
        numberOfPlayers = numPlayers;
    }
}
