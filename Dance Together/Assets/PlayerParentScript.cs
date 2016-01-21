using UnityEngine;
using System.Collections;

public class PlayerParentScript : MonoBehaviour
{
    private bool isRotating;

    private float offsetAngle;

    private Collider2D myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
    }
    void Update()
    {


        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider == myCollider) {
                // rotating flag
                isRotating = true;

                Vector3 mouse_pos = Input.mousePosition;
                Vector3 object_pos = Camera.main.WorldToScreenPoint(transform.position);
                float dx = mouse_pos.x - object_pos.x;
                float dy = mouse_pos.y - object_pos.y;
                offsetAngle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg - transform.rotation.eulerAngles.z;
            }
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            isRotating = false; //Always false on mouse up
        }
        else if (Input.GetButton("Fire1"))
        {
            if (isRotating)
            {
                Vector3 mouse_pos = Input.mousePosition;
                Vector3 object_pos = Camera.main.WorldToScreenPoint(transform.position);
                float dx = mouse_pos.x - object_pos.x;
                float dy = mouse_pos.y - object_pos.y;
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - offsetAngle));
            }
        }
    }

}
