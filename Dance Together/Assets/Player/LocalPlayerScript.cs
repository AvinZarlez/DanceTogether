using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [SyncVar,HideInInspector]
    public float captainsCountdown = 0;
    
    // To make referencing easier/less calls.
    private PlayerParentScript playerParentScript;
    private NetworkedPlayerScript networkedPScript;
    private Text countdownText; // UI text object named "UI_Countdown"
    private Text infoText; // UI text object named "UI_InfoText"
    private Text detailsText; // UI text object named "UI_DetailsText"

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

        GameObject obj3 = GameObject.Find("UI_DetailsText");
        detailsText = obj3.GetComponent<Text>();
        detailsText.enabled = false;

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
        List<CaptainsMessPlayer> players = networkedPScript.GetPlayers();
        foreach (CaptainsMessPlayer player in players)
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

        if (GameManagerScript.instance.IsInPostGame())
        {
            countdownText.enabled = false;
            infoText.enabled = true; // Redundent?
            detailsText.enabled = true;

            int song = networkedPScript.GetSongID();
            int match = networkedPScript.GetMatchSongID();
            if (song == match)
            {
                infoText.text = "You Won!";
                detailsText.text = "You both were dancing to " + song.ToString();
            }
            else
            {
                infoText.text = "You Lost!";
                detailsText.text = "You danced to " + song.ToString() + "\nThey danced to " + match.ToString();
            }
        }
        else
        {
            detailsText.enabled = false;

            float countDown = GameManagerScript.instance.countDown;

            if (networkedPScript.isServer)
                captainsCountdown = networkedPScript.mess.CountdownTimer();

            if (captainsCountdown > 0)
            {
                infoText.enabled = true;

                //Always false during intro countdown.
                isHit = false;
                isDragging = false;

                if (captainsCountdown < 1)
                {
                    infoText.text = "DANCE!";
                }
                else if (captainsCountdown >= (4f)) //Plus one second for the "Dance" end 
                {
                    infoText.text = "Ready?";
                }
                else
                {
                    infoText.text = "" + Mathf.Floor(captainsCountdown);
                }
            }
            else if (countDown > 0)
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

                                    List<CaptainsMessPlayer> players = networkedPScript.GetPlayers();
                                    foreach (CaptainsMessPlayer player in players)
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
                infoText.enabled = false;

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
                        networkedPScript.ToggleReady();
                    }
                    //Always false after mouse up
                    isHit = false;
                    //isDragging = false; //Set above, always not dragging.
                }
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
