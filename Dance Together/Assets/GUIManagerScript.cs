using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIManagerScript : MonoBehaviour {
    
    private static GameObject gameButtonObject;

    private static GameObject backButtonObject;

    private static Button gameButton;

    private static Text buttonText;

    // Use this for initialization
    void Start ()
    {
        backButtonObject = GameObject.Find("UI_BackButton");
        gameButtonObject = GameObject.Find("UI_GameButton");
        gameButton = gameButtonObject.GetComponent<Button>();
        buttonText = gameButtonObject.GetComponentInChildren<Text>();

        SetButton(false);
        SetBackButton(false);
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void MainButtonPressed()
    {
        GameObject gm = GameObject.Find("LOCAL Player");
        gm.GetComponent<NetworkedPlayerScript>().MainButtonPressed();
    }

    public void BackButtonPressed()
    {
        GameObject gm = GameObject.Find("LOCAL Player");
        gm.GetComponent<LocalPlayerScript>().BackButtonPressed();
    }

    public static void SetButton(bool enabled)
    {
        gameButtonObject.SetActive(enabled);
    }

    public static void SetButtonText(string s)
    {
        buttonText.text = s;
    }

    /*
    public static void SetButtonInteractable(bool enabled)
    {
        gameButton.interactable = enabled;
    }
    */

    public static void SetReplayButton(bool enabled)
    {
        if (enabled)
        {
            SetButton(true);
            //SetButtonInteractable(true);
            SetButtonText("Replay");
        }
        else
        {
            //SetButtonInteractable(false);
            SetButtonText("Dance");
        }
    }

    private static void SetBackButton(bool enabled)
    {
        backButtonObject.SetActive(enabled);
    }

    public static void SetMatchButton(bool enabled)
    {
        SetBackButton(enabled);
        if (enabled)
        {
            SetButton(true);
            //SetButtonInteractable(true);
            SetButtonText("Match");
        }
        else
        {
            SetButton(false);
            //SetButtonInteractable(false);
            SetButtonText("Dance"); // Not needed? Redundant?
        }
    }
}
