using UnityEngine;
using System.Collections;

public class PlayerParentScript : MonoBehaviour
{
    private bool isRotating;

    private float offsetAngle;

    void Update()
    {
        if (isRotating)
        {
            Vector3 mouse_pos = Input.mousePosition;
            Vector3 object_pos = Camera.main.WorldToScreenPoint(transform.position);
            float dx = mouse_pos.x - object_pos.x;
            float dy = mouse_pos.y - object_pos.y;
            float angle = Mathf.Atan2(dy,dx) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - offsetAngle));
        }
    }

    void OnMouseDown()
    {
        // rotating flag
        isRotating = true;

        Vector3 mouse_pos = Input.mousePosition;
        Vector3 object_pos = Camera.main.WorldToScreenPoint(transform.position);
        float dx = mouse_pos.x - object_pos.x;
        float dy = mouse_pos.y - object_pos.y;
        offsetAngle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg - transform.rotation.eulerAngles.z;

    }  

    void OnMouseUp()
    {
        // rotating flag
        isRotating = false;
    }

}
