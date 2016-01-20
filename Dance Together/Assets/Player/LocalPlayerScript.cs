using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocalPlayerScript : MonoBehaviour {
    [System.NonSerialized] // Don't need to save the isDragging state
    public bool isDragging; // Is the player object currently being dragged somewhere?

    [HideInInspector] // Don't need to see the starting location
    public Vector3 startingLocation; // Where the player object starts

    private Text countdownText; // UI text object named "UI_Countdown"

    [SerializeField] //Make this seen in the editor, but still private/local to this class.
    private float movementSpeed = 10; // How fast does it snap back to the center?

    void Start()
    {
        GameObject obj = GameObject.Find("UI_Countdown");
        countdownText = obj.GetComponent<Text>();
        startingLocation = transform.position;
    }

    void OnMouseDown()
    {
        if (isActiveAndEnabled)
        {
            isDragging = true;


        }
    }

    void OnMouseDrag()
    {
        if (isActiveAndEnabled)
        {
            Vector3 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mouse_position.x, mouse_position.y, transform.position.z);
        }
    }

    void OnMouseUp()
    {
        //if (isActiveAndEnabled) {
            isDragging = false;
        //}
    }

    void Update()
    {
        countdownText.text = ""+this.GetComponent<NetworkedPlayerScript>().countDown;
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
