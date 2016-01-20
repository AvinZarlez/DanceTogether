using UnityEngine;
using System.Collections;

public class RemotePlayerScript : MonoBehaviour {
    [SerializeField]
    int distance = 3;
    [SerializeField]
    int movementSpeed = 5;

    int position;
    int numberOfPlayers;

    //GameObject localPlayer;
    Vector3 localPlayerStartingPosition;

    void Start()
    {
        GameObject localPlayer = GameObject.Find("LOCAL Player");
        localPlayerStartingPosition = localPlayer.GetComponent<LocalPlayerScript>().startingLocation;
    }

    public void SetPosition(int p, int numPlayers)
    {
        position = p;
        numberOfPlayers = numPlayers;
    }

    void Update()
    {
        float step = movementSpeed * Time.deltaTime;
        Vector3 goal = localPlayerStartingPosition;
        float degreeMath = position * (2 * Mathf.PI) / (numberOfPlayers - 1);
        goal.x += distance * Mathf.Cos(degreeMath);
        goal.y += distance * Mathf.Sin(degreeMath);
        transform.position = Vector3.MoveTowards(transform.position, goal, step);
    }
}
