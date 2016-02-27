using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIManagerScript : MonoBehaviour {
    
    private static GameObject gameButtonObject;

    private static Button gameButton;

    private static Text buttonText;

    // Use this for initialization
    void Start () {
        gameButtonObject = GameObject.Find("UI_GameButton");
        gameButton = gameButtonObject.GetComponent<Button>();
        buttonText = gameButtonObject.GetComponentInChildren<Text>();

        SetButton(false);
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void MainButtonPressed()
    {
        GameObject gm = GameObject.Find("LOCAL Player");
        gm.GetComponent<NetworkedPlayerScript>().CmdMainButtonPressed();
    }

    public static void SetButton(bool enabled)
    {
        gameButtonObject.SetActive(enabled);
    }

    public static void SetButtonInteractable(bool enabled)
    {
        gameButton.interactable = enabled;
    }

    public static void SetReplyButton(bool enabled)
    {
        if (enabled)
        {
            SetButton(true);
            SetButtonInteractable(true);
            buttonText.text = "Reply";
        }
        else
        {
            SetButtonInteractable(false);
            buttonText.text = "Dance";
        }
    }
}
