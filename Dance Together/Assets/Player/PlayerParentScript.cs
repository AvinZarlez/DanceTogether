using UnityEngine;
using System.Collections;

public class PlayerParentScript : MonoBehaviour
{
    private bool isRotating;

    private float offsetAngle;

    private Collider2D myCollider;

    private float spinLocked = -1;

    private bool spinning = false;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (spinLocked >= 0)
        {
            if (spinning)
            {
                transform.Rotate(new Vector3(0, 0, 180) * Time.deltaTime);
                spinLocked += Time.deltaTime;
                if (spinLocked > 4)
                {
                    spinLocked = -1;
                    spinning = false;
                }
            }
        }
        else
        {
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

    public void LockAndSpin()
    {
        spinLocked = 0;
        spinning = true;
    }

    public void Lock()
    {
        spinLocked = 0;
        spinning = false;
    }

    public void Unlock()
    {
        spinLocked = -1;
        spinning = false;
    }
}
