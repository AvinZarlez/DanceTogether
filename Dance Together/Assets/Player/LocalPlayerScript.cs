using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocalPlayerScript : MonoBehaviour {
    [System.NonSerialized] // Don't need to save the isDragging state
    public bool isDragging; // Is the player object currently being dragged somewhere?

    [HideInInspector] // Don't need to see the starting location, does need to be public so RemotePlayerScript can get it.
    public Vector3 startingLocation; // Where the player object starts

    private Text countdownText; // UI text object named "UI_Countdown"

    [SerializeField] //Make this seen in the editor, but still private/local to this class.
    private float movementSpeed = 10; // How fast does it snap back to the center?

    private bool isHit; //Did the TouchDown event happen?

    private NetworkedPlayerScript lastCollidedWith; //Direct link to the NetworkedPlayerScript of the object we last collided with.\

    // To make referencing easier/less calls.
    private NetworkedPlayerScript networkedPScript;

    void Start()
    {
        networkedPScript = GetComponent<NetworkedPlayerScript>();

        GameObject obj = GameObject.Find("UI_Countdown");
        countdownText = obj.GetComponent<Text>();

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
        float countDown = networkedPScript.countDown;

        if (countDown > 0) //No interaction during count down
        {
            countDown -= Time.deltaTime;
            networkedPScript.countDown = countDown;

            if (networkedPScript.GetIsGameStarted())
            {

                if (countDown <= 0)
                {
                    countdownText.text = "GAME OVER!" + " " + networkedPScript.GetSongID(); ;
                    // TEMP TODO This gets triggered X times where X is the number of places. Should fix.
                    networkedPScript.CmdEndGame(); //Let's stop this party

                    //Always false when time is up
                    isHit = false;
                    isDragging = false;
                }
                else
                {
                    // The main game, in the middle of game play
                    // This. is. it. - TIME TO DANCE!
                    countdownText.text = "" + Mathf.Ceil(countDown);

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

            }
            else // countDown was > 0 AND Game is not started yet.
            {
                //Always false during intro countdown.
                isHit = false;
                isDragging = false; 

                if (countDown <= 0)
                {
                    countdownText.text = "";
                    networkedPScript.StartMainGame(); //Let's get this party started;
                }
                else if (countDown < 1)
                {
                    countdownText.text = "DANCE!";
                }
                else if (countDown >= (4f)) //Plus one second for the "Dance" end 
                {
                    countdownText.text = "Ready?";
                }
                else
                {
                    countdownText.text = "" + Mathf.Floor(countDown);
                }
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
