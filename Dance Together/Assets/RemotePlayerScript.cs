using UnityEngine;
using System.Collections;

public class RemotePlayerScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void MovePlayer ()
    {
        transform.position = new Vector3(transform.position.x+4, transform.position.y, transform.position.z);
    }
}
