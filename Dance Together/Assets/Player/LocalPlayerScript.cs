using UnityEngine;
using System.Collections;

public class LocalPlayerScript : MonoBehaviour {
    [System.NonSerialized] // Don't need to save the isDragging state
    public bool isDragging; // Is the player object currently being dragged somewhere?

    [HideInInspector] // Don't need to see the starting location, does need to be public so RemotePlayerScript can get it.
    public Vector3 startingLocation; // Where the player object starts

    [SerializeField] //Make this seen in the editor, but still private/local to this class.
    private float movementSpeed = 10; // How fast does it snap back to the center?

    private bool isHit; //Did the TouchDown event happen?

    private NetworkedPlayerScript lastCollidedWith; //Direct link to the NetworkedPlayerScript of the object we last collided with.\

    // To make referencing easier/less calls.
    private NetworkedPlayerScript networkedPScript;

    void Start()
    {
        networkedPScript = GetComponent<NetworkedPlayerScript>();

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        startingLocation = transform.position;

        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        lastCollidedWith = other.GetComponent<NetworkedPlayerScript>();
    }

    void Update()
    {
        float countDown = GameManagerScript.instance.countDown;

        if (countDown > 0) //No interaction during count down
        {
            if (GameManagerScript.instance.IsInMainGameplay())
            {
                // The main game, in the middle of game play
                // This. is. it. - TIME TO DANCE!

                if (Input.GetButtonDown("Fire1"))
                {
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (GetComponent<Collider2D>().OverlapPoint(mousePosition))
                    {
                        isDragging = true;
                    }
                }
                else if (Input.GetButtonUp("Fire1"))
                {
                    if (GetComponent<Collider2D>().IsTouching(lastCollidedWith.GetComponent<Collider2D>()) && isDragging)
                    {
                        networkedPScript.CmdStartGame();
                    }

                    //Always false after mouse up
                    isHit = false;
                    isDragging = false;
                }
                else if (Input.GetButton("Fire1"))
                {
                    if (isDragging)
                    {
                        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
                    }
                }

            }
            else // countDown was > 0 AND Game is not in main gameplay
            {
                //Always false during intro countdown.
                isHit = false;
                isDragging = false;
            }

        }
        else
        {
            isDragging = false; //Always false if no countdown.

            if (Input.GetButtonDown("Fire1"))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (GetComponent<Collider2D>().OverlapPoint(mousePosition))
                {
                    isHit = true;
                }
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (GetComponent<Collider2D>().OverlapPoint(mousePosition) && isHit)
                {
                    networkedPScript.CmdToggleReady();
                }
                //Always false after mouse up
                isHit = false;
                //isDragging = false; //Set above, always not dragging.
            }
        }

        if (isDragging) {
            // Player object is being dragged right now
        }
        else {
            // Player object is NOT being dragged

            // Move back to the center.
            float step = movementSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, startingLocation, step);
        }
    }
}
