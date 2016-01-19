using UnityEngine;
using System.Collections;

public class LocalPlayerScript : MonoBehaviour {
    public float movement_speed; // How fast does it snap back to the center?

    [System.NonSerialized] // Don't need to save the isDragging state
    public bool isDragging; // Is the player object currently being dragged somewhere?

    private Vector3 starting_location; // Where the player object starts

    void Start()
    {
        starting_location = transform.position;
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseDrag()
    {
        Vector3 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mouse_position.x, mouse_position.y, transform.position.z);
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (isDragging) {
            // Player object is being dragged right now
        }
        else {
            // Player object is NOT being dragged

            // Move back to the center.
            float step = movement_speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, starting_location, step);
        }
    }
}
