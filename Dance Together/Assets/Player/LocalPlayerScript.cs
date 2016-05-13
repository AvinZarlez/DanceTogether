using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LocalPlayerScript : MonoBehaviour
{
    [HideInInspector] // Don't need to see the starting location, does need to be public so RemotePlayerScript can get it.
    public Vector3 startingLocation; // Where the player object starts

    [SerializeField] //Make this seen in the editor, but still private/local to this class.
    private float movementSpeed = 10; // How fast does it snap back to the center?

    private NetworkedPlayerScript lastCollidedWith; //Direct link to the NetworkedPlayerScript of the object we last collided with.
    
    // To make referencing easier/less calls.
    //private PlayerParentScript playerParentScript;
    private NetworkedPlayerScript networkedPScript;
    private Text countdownText; // UI text object named "UI_Countdown"
    private Text infoText; // UI text object named "UI_InfoText"
    private Text detailsText; // UI text object named "UI_DetailsText"

    void Start()
    {
        //playerParentScript = GameObject.FindWithTag("PlayerParent").GetComponent<PlayerParentScript>();

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


        Application.runInBackground = true;
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        lastCollidedWith = other.GetComponent<NetworkedPlayerScript>();
    }
    
    public void BackButtonPressed()
    {
        // Nothing happens
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
                infoText.text = "You Won with "+ Mathf.Floor(networkedPScript.matchTime);
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

            float captainsCountdown = networkedPScript.captainsCountdown;
            if (captainsCountdown > 0)
            {
                infoText.enabled = true;

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
            }
            else
            {
                infoText.enabled = false;
            }
        }
    }
}
