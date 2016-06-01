using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIManagerScript : MonoBehaviour {
    
    private static GameObject gameButtonObject;

    private static GameObject backButtonObject;

    private static Button gameButton;

    private static Text buttonText;

    private static Text scoreText;

    private static GameObject nameInputObject;

    public static Text countdownText; // UI text object named "UI_Countdown"
    public static Text infoText; // UI text object named "UI_InfoText"
    public static Text detailsText; // UI text object named "UI_DetailsText"

    // Use this for initialization
    void Start ()
    {
        backButtonObject = GameObject.Find("UI_BackButton");
        gameButtonObject = GameObject.Find("UI_GameButton");
        gameButton = gameButtonObject.GetComponent<Button>();
        buttonText = gameButtonObject.GetComponentInChildren<Text>();

        nameInputObject = GameObject.Find("UI_NameInput");
        SetInput(false);

        SetButton(false);
        SetBackButton(false);

        scoreText = GameObject.Find("UI_Score").GetComponent<Text>();
        scoreText.enabled = false;
        
        GameObject obj1 = GameObject.Find("UI_Countdown");
        countdownText = obj1.GetComponent<Text>();
        countdownText.enabled = false;

        GameObject obj2 = GameObject.Find("UI_InfoText");
        infoText = obj2.GetComponent<Text>();
        infoText.enabled = false;

        GameObject obj3 = GameObject.Find("UI_DetailsText");
        detailsText = obj3.GetComponent<Text>();
        detailsText.enabled = false;
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
    
    public void SetPlayerText()
    {
        InputField field = nameInputObject.GetComponent<InputField>();

        GameObject player = GameObject.Find("LOCAL Player");
        NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
        nps.CmdSetPlayerText(field.text);
    }

    public static void SetButton(bool enabled)
    {
        if (gameButtonObject != null)
            gameButtonObject.SetActive(enabled);
    }

    public static void SetInput(bool enabled)
    {
        if (nameInputObject != null)
            nameInputObject.SetActive(enabled);
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

    public static void SetBackButton(bool enabled)
    {
        if (backButtonObject != null)
            backButtonObject.SetActive(enabled);
    }

    public static void SetScoreText(int score)
    {
        scoreText.enabled = true;
        scoreText.text = "Score:\n" + score.ToString();
    }
}
