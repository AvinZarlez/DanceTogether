using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocalPlayerScript : MonoBehaviour
{
    [System.NonSerialized] // Don't need to save the isDragging state
    public bool isDragging; // Is the player object currently being dragged somewhere?

    [HideInInspector] // Don't need to see the starting location, does need to be public so RemotePlayerScript can get it.
    public Vector3 startingLocation; // Where the player object starts

    [SerializeField] //Make this seen in the editor, but still private/local to this class.
    private float movementSpeed = 10; // How fast does it snap back to the center?

    private bool isHit; //Did the TouchDown event happen?
    private bool locked;

    private NetworkedPlayerScript lastCollidedWith; //Direct link to the NetworkedPlayerScript of the object we last collided with.

    [HideInInspector]
    public int choiceSongID; // The player this player is thinking about being a match.

    // To make referencing easier/less calls.
    private PlayerParentScript playerParentScript;
    private NetworkedPlayerScript networkedPScript;
    private Text countdownText; // UI text object named "UI_Countdown"
    private Text infoText; // UI text object named "UI_InfoText"

    void Start()
    {
        playerParentScript = GameObject.FindWithTag("PlayerParent").GetComponent<PlayerParentScript>();

        networkedPScript = GetComponent<NetworkedPlayerScript>();

        GameObject obj1 = GameObject.Find("UI_Countdown");
        countdownText = obj1.GetComponent<Text>();
        countdownText.enabled = false;

        GameObject obj2 = GameObject.Find("UI_InfoText");
        infoText = obj2.GetComponent<Text>();
        infoText.enabled = false;

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        startingLocation = transform.position;

        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        choiceSongID = -1;
        locked = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        lastCollidedWith = other.GetComponent<NetworkedPlayerScript>();
    }

    public bool WasMatchedPressed()
    {
        if (locked)
        {
            return true;
        }
        return false;
    }
    
    public void BackButtonPressed()
    {
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<RemotePlayerScript>().Reset();
        }

        playerParentScript.Unlock();

        choiceSongID = -1;

        locked = false;
        GUIManagerScript.SetMatchButton(false);
    }

    void Update()
    {
        float countDown = GameManagerScript.instance.countDown;

        if (GameManagerScript.instance.IsInPostGame())
        {
            countdownText.enabled = false;
            infoText.enabled = true; // Redundent?

            int song = networkedPScript.GetSongID();
            int match = networkedPScript.GetMatchSongID();
            infoText.text = "GAME OVER!" + " Was:" + song + " Picked:" + match + " ";
            if (song == match)
            {
                infoText.text = infoText.text + "You Win!!!";
            }
            else
            {
                infoText.text = infoText.text + "Fail!!!";
            }

        }
        else if (countDown > 0)
        {
            if (GameManagerScript.instance.IsInMainGameplay())
            {
                infoText.enabled = false;
                countdownText.enabled = true;
                countdownText.text = "" + Mathf.Ceil(countDown);

                // The main game, in the middle of game play
                // This. is. it. - TIME TO DANCE!

                if (!locked)
                {
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
                        if (lastCollidedWith != null)
                        {
                            if (GetComponent<Collider2D>().IsTouching(lastCollidedWith.GetComponent<Collider2D>()) && isDragging)
                            {
                                choiceSongID = lastCollidedWith.GetComponent<NetworkedPlayerScript>().GetSongID();

                                lastCollidedWith.remotePScript.localPlayer = this.gameObject;

                                GameObject[] players;
                                players = GameObject.FindGameObjectsWithTag("Player");
                                foreach (GameObject player in players)
                                {
                                    player.GetComponent<RemotePlayerScript>().highlighted = true;
                                }

                                playerParentScript.Lock();

                                locked = true;
                                GUIManagerScript.SetMatchButton(true);
                            }
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
            else
            {
                infoText.enabled = true;

                //Always false during intro countdown.
                isHit = false;
                isDragging = false;

                if (countDown < 1)
                {
                    infoText.text = "DANCE!";
                }
                else if (countDown >= (4f)) //Plus one second for the "Dance" end 
                {
                    infoText.text = "Ready?";
                }
                else
                {
                    infoText.text = "" + Mathf.Floor(countDown);
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

        if (isDragging)
        {
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
