using UnityEngine;
using System.Collections;

public class RemotePlayerScript : MonoBehaviour {
    [SerializeField]
    int distance = 3;
    [SerializeField]
    int movementSpeed = 5;

    int position;
    int numberOfPlayers;

    public bool growing;

    private Transform body; //The Sphere

    void Start()
    {
        body = transform.Find("Sphere");
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
        goal.z -= 1;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, goal, step);
    }

    void FixedUpdate()
    {
        // Grow as player overlaps
        if (growing)
        {
            if (body.localScale.x < 2)
            {
                body.localScale += new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
        else
        {
            if (body.localScale.x > 1) {
                body.localScale -= new Vector3(0.15f, 0.15f, 0.15f);
            }
        }
    }

    public void SetPosition(int p, int numPlayers)
    {
        // Set our position, starts at 1 and goes up to (and including) numPlayers
        position = p;
        numberOfPlayers = numPlayers;
    }
}
