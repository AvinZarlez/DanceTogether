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

    bool GetIsGameStarted() {
        return networkedPScript.GetIsGameStarted();
    }

    void Update()
    {
        bool gameStarted = GetIsGameStarted();
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if (Input.GetButtonDown("Fire1"))
        {
            if (GetComponent<Collider2D>().OverlapPoint(mousePosition))
            {
                isHit = true;
                if (gameStarted)
                {
                    isDragging = true;
                }
                else
                {
                    networkedPScript.CmdToggleReady();
                }
            }
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            if (gameStarted)
            {

            }
            else if (GetComponent<Collider2D>().OverlapPoint(mousePosition) && isHit)
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
                Vector3 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(mouse_position.x, mouse_position.y, transform.position.z);
            }
        }

        countdownText.text = ""+networkedPScript.GetSongID();

        float countDown = networkedPScript.countDown;

        if (countDown > 0)
        {
            countDown -= Time.deltaTime;

            if (countDown <= 0)
            {
                countdownText.text = "";
            }
            else if (countDown < 1)
            {
                countdownText.text = "DANCE!";
            }
            else
            {
                countdownText.text = "" + Mathf.Floor(countDown);
            }

            networkedPScript.countDown = countDown;
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
