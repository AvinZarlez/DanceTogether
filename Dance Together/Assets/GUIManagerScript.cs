using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections;

public class GUIManagerScript : MonoBehaviour {
    
    private static GameObject gameButtonObject;

    private static GameObject rulesButtonObject;

    private static GameObject backButtonObject;

    //private static Button gameButton;

    private static Text buttonText;

    private static Text scoreText;
    
    private static Button resetScoreBtn; //Changed to stay in view, part of top menu now.

    private static GameObject numberObject;
    private static GameObject colorShowObject;

    private static Text whichColorText;
    private static Image whichColorPanel;

    public static Text countdownText; // UI text object named "UI_Countdown"
    public static Text infoText; // UI text object named "UI_InfoText"
    public static Text sliderText; // UI text object named "UI_SliderText"

    public static Text finalScoreText; // UI text object named "UI_FinalScore"
    public static Text answerText; // UI text object named "UI_Answer"
    public static Text noGuessText; // UI text object named "UI_NoGuess"
    public static Text listeningToText; // UI text object named "UI_ListeningTo"
    public static Text detailsText; // UI text object named "UI_DetailsText"
    public static Text continuingInText; // UI text object named "UI_ContinuingIn"
    public static GameObject playerPickedBtn; // UI Button named "UI_WhichPlayerPicked"
    public static GameObject lookingForBtn; // UI Button named "UI_LookingForPlayer"
    public static GameObject lookingForParent;
    public static GameObject answerParent;

    public static GameObject playerParent;
    private static GameObject playerSliderParent;
    private static GameObject endGameParent;

    private static Renderer bgRenderer;

    public static Text versionText;

    public static InputField numberTextField;

    // Use this for initialization
    void Start ()
    {
        versionText = GameObject.Find("UI_Version").GetComponent<Text>();

        numberTextField = GameObject.Find("UI_NumberInputField").GetComponent<InputField>();

        gameButtonObject = GameObject.Find("UI_GameButton");
        //gameButton = gameButtonObject.GetComponent<Button>();
        buttonText = gameButtonObject.GetComponentInChildren<Text>();
        SetMainButtonHighlight(false);
        SetButton(false);
        
        rulesButtonObject = GameObject.Find("UI_RulesButton");
        SetRulesButton(false);

        backButtonObject = GameObject.Find("UI_BackButton");
        SetBackButton(false);

        whichColorPanel = GameObject.Find("UI_WhichColorPanel").GetComponent<Image>();

        numberObject = GameObject.Find("NumberParent");
        whichColorText = GameObject.Find("UI_WhichColor").GetComponent<Text>();
        SetInput(false);

        colorShowObject = GameObject.Find("ColorShowParent");
        HideColorShow();

        scoreText = GameObject.Find("UI_Score").GetComponent<Text>();
        scoreText.enabled = false;

        resetScoreBtn = GameObject.Find("UI_ResetScore").GetComponent<Button>();
        resetScoreBtn.interactable = false;

        // Dump solution to getting resetScoreBtn before the top menu parent is hidden?
        GameObject.Find("TopMenuParent").SetActive(false);

        GameObject obj1 = GameObject.Find("UI_Countdown");
        countdownText = obj1.GetComponent<Text>();
        countdownText.enabled = false;

        GameObject obj2 = GameObject.Find("UI_InfoText");
        infoText = obj2.GetComponent<Text>();
        infoText.enabled = false;

        GameObject obj4 = GameObject.Find("UI_SliderText");
        sliderText = obj4.GetComponent<Text>();

        bgRenderer = GameObject.Find("Game_Background").GetComponent<Renderer>();

        playerParent = GameObject.FindWithTag("PlayerParent");
        
        finalScoreText = GameObject.Find("UI_FinalScore").GetComponent<Text>();
        answerText = GameObject.Find("UI_Answer").GetComponent<Text>();
        noGuessText = GameObject.Find("UI_NoGuess").GetComponent<Text>();
        listeningToText = GameObject.Find("UI_ListeningTo").GetComponent<Text>();
        detailsText = GameObject.Find("UI_DetailsText").GetComponent<Text>();
        continuingInText = GameObject.Find("UI_ContinuingIn").GetComponent<Text>();
        playerPickedBtn = GameObject.Find("UI_WhichPlayerPicked");
        lookingForBtn = GameObject.Find("UI_LookingForPlayer");

        answerParent = GameObject.Find("AnswerParent");
        lookingForParent = GameObject.Find("LookingForParent");

        playerSliderParent = GameObject.Find("PlayerSliderParent");
        endGameParent = GameObject.Find("EndGameParent");
        HideMainView();
    }

    // Update is called once per frame
    void Update () {

    }

    public void MainButtonPressed()
    {
        GameObject gm = GameObject.Find("LOCAL Player");
        gm.GetComponent<NetworkedPlayerScript>().MainButtonPressed();
    }

    public void RulesButtonPressed()
    {
        GameObject gm = GameObject.Find("LOCAL Player");
        gm.GetComponent<NetworkedPlayerScript>().CmdRulesButtonPressed();
    }

    public void BackButtonPressed()
    {
        GameObject gm = GameObject.Find("LOCAL Player");
        gm.GetComponent<LocalPlayerScript>().BackButtonPressed();
    }

    public static void FillPlayerNumber(int n)
    {
        Text field = numberObject.transform.Find("UI_NumberOfPlayer").GetComponent<Text>();
        field.text = n.ToString();
    }

    public static void SetButton(bool enabled)
    {
        if (gameButtonObject != null)
        {
            gameButtonObject.SetActive(enabled);
        }
    }

    public static void SetInput(bool enabled)
    {
        if (numberObject != null)
            numberObject.SetActive(enabled);
    }

    public static void SetInputColor(Color c, string name)
    {
        if (numberObject != null)
            numberObject.transform.Find("UI_NumberColorPanel").GetComponent<Image>().color = c;
        if (whichColorText != null)
            whichColorText.text = name;
        if (whichColorPanel != null)
            whichColorPanel.color = c;
    }

    public static void SetButtonText(string s)
    {
        if (s == "Dance")
        {
            // Hacky bullshit that might end up being too many calls? But hopefully not?
            GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
            Assert.IsNotNull<GameManagerScript>(gameManager);
            if (gameManager.GetRoundCount() > 0) { s = "Replay"; }
        }

        buttonText.text = s;
    }

    /*
    public static void SetButtonInteractable(bool enabled)
    {
        gameButton.interactable = enabled;
    }
    */

    public static void SetBackButton(bool enabled)
    {
        if (backButtonObject != null)
            backButtonObject.SetActive(enabled);
    }

    public static void SetScoreText(int score)
    {
        scoreText.enabled = true;
        scoreText.text = "Score:\n" + score.ToString();

        resetScoreBtn.interactable = true;
    }

    public static void HideScoreText()
    {
        scoreText.enabled = false;

        resetScoreBtn.interactable = false;
    }

    public static void SetMainButtonHighlight(bool highlight)
    {
        gameButtonObject.GetComponent<Outline>().enabled = highlight;
    }

    public static void SetRulesButton(bool enabled)
    {
        if (rulesButtonObject != null)
        {
            rulesButtonObject.SetActive(enabled);
        }
    }

    public static void SetBGColor(Color c)
    {
        bgRenderer.material.SetColor("_Color", c);
    }

    public static void SetColorShow(int number, Color c, string color_name)
    {
        if (colorShowObject != null)
        {
            colorShowObject.SetActive(true);
            colorShowObject.transform.Find("UI_ColorShowColorPanel").GetComponent<Image>().color = c;
            string txt = number + "\n(" + color_name +")";
            colorShowObject.transform.Find("UI_ColorShowPlayerName").GetComponent<Text>().text = txt;
        }
    }

    public static void HideColorShow()
    {
        if (colorShowObject != null)
            colorShowObject.SetActive(false);
    }
    
    public static void SetEndGameScreen(bool b)
    {
        if (b) //Yes to end game screen
        {
            if (playerSliderParent != null)
                playerSliderParent.SetActive(false);
            if (endGameParent != null)
                endGameParent.SetActive(true);
        }
        else
        {
            if (playerSliderParent != null)
                playerSliderParent.SetActive(true);
            if (endGameParent != null)
                endGameParent.SetActive(false);
        }
    }

    public static void HideMainView()
    {
        if (playerSliderParent != null)
            playerSliderParent.SetActive(false);
        if (endGameParent != null)
            endGameParent.SetActive(false);
    }

    public static void SetNumberInputFieldColor(Color c)
    {
        numberTextField.GetComponent<Image>().color = c;
    }

    public static void ClearNumberInput()
    {
        numberTextField.text = "";
    }
}
