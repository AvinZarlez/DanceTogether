using UnityEngine;
using System.Collections;

public class RemotePlayerScript : MonoBehaviour {
    [SerializeField]
    int distance = 3;
    [SerializeField]
    float movementSpeed = 5.0f;

    int position;
    int numberOfPlayers;

    [HideInInspector]
    public bool growing;
    [HideInInspector]
    public bool highlighted = false;
    [HideInInspector]
    public GameObject localPlayer;

    private Vector3 goal;
    private Vector3 highlighted_goal;

    private Transform body; //The Sphere

    void Start()
    {
        body = transform.Find("Sphere");
    }

    void FixedUpdate()
    {
        /*
        // Grow as player overlaps
        if ( ((!highlighted)&&(growing)) || (highlighted&&(localPlayer != null)) )
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

        if (highlighted)
        {
            Vector3 new_goal;
            if (localPlayer != null)
            {
                new_goal = new Vector3(0,-distance);
            }
            else
            {
                new_goal = highlighted_goal;
            }
            transform.position = Vector3.MoveTowards(transform.position, new_goal, movementSpeed);
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, goal, movementSpeed);
        }*/
    }

    public void Reset()
    {
        growing = false;
        highlighted = false;
        localPlayer = null;
    }

    public void SetPosition(int p, int numPlayers)
    {
        // Set our position, starts at 1 and goes up to (and including) numPlayers
        position = p;
        numberOfPlayers = numPlayers;
        
        goal = Vector3.zero;
        float degreeMath;
        if (numberOfPlayers > 2) degreeMath = position * (2 * Mathf.PI) / (numberOfPlayers - 1);
        else degreeMath = Mathf.PI; //Cheat if 2 players. If theory this won't happen if only one player, but just in case also prevents divide by 0.
        goal.x += distance * Mathf.Cos(degreeMath);
        goal.y += distance * Mathf.Sin(degreeMath);
        goal.z -= 1;

        highlighted_goal = new Vector3(-(((numberOfPlayers - 2) * 1.25f) / 2) + (1.25f * (position - 1f)), 3);
    }
}
