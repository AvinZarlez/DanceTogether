using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIManagerScript : MonoBehaviour {
    
    private static GameObject gameButtonObject;

    private static Button gameButton;

    // Use this for initialization
    void Start () {
        gameButtonObject = GameObject.Find("UI_GameButton");
        gameButton = gameButtonObject.GetComponent<Button>();

        SetButton(false);
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void StartGame()
    {
        GameObject gm = GameObject.Find("LOCAL Player");
        gm.GetComponent<NetworkedPlayerScript>().CmdStartGame();
    }

    public static void SetButton(bool enabled)
    {
        gameButtonObject.SetActive(enabled);
    }

    public static void SetButtonInteractable(bool enabled)
    {
        gameButton.interactable = enabled;
    }
}
