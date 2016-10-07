using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using DG.Tweening;
using System.Collections.Generic;

public class LocalPlayerScript : MonoBehaviour
{
    // To make referencing easier/less calls.
    //private PlayerParentScript playerParentScript;
    private NetworkedPlayerScript networkedPScript;
    private Text countdownText; // UI text object named "UI_Countdown"
    private Text sliderText; // UI text object named "UI_SliderText"

    private Text infoText; // UI text object named "UI_InfoText"

    private Text finalScoreText; // UI text object named "UI_FinalScore"
    private Text answerText; // UI text object named "UI_Answer"
    private Text listeningToText; // UI text object named "UI_ListeningTo"
    private Text detailsText; // UI text object named "UI_DetailsText"
    private Text continuingInText; // UI text object named "UI_ContinuingIn"

    private Button playerPickedBtn; // UI Button named "UI_WhichPlayerPicked"
    private Button lookingForBtn; // UI Button named "UI_LookingForPlayer"

    public bool reminded = false;

    void Start()
    {
        //playerParentScript = GameObject.FindWithTag("PlayerParent").GetComponent<PlayerParentScript>();

        networkedPScript = GetComponent<NetworkedPlayerScript>();
        
        countdownText = GUIManagerScript.countdownText;

        sliderText = GUIManagerScript.sliderText;

        infoText = GUIManagerScript.infoText;

        // Assigned GUI elements based on
        // GUIManagerScript.endGameParent
        GameObject endGameParent = GUIManagerScript.endGameParent;
        answerText = endGameParent.transform.Find("UI_Answer").GetComponent<Text>();

        Application.runInBackground = true;
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    
    public void BackButtonPressed()
    {
        // Oops, let's reset the match ID
        networkedPScript.CmdResetMatchSongID();
        
        List<CaptainsMessPlayer> players = networkedPScript.GetPlayers();

        int size = players.Count;

        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
            if (!nps.isLocalPlayer)
            {
                nps.playerButton.SetActive(true);
                nps.playerButton.GetComponent<Button>().interactable = true;

                Vector3 goal = player.GetComponent<RemotePlayerScript>().GetPosition();

                nps.playerButton.transform.DOLocalMove(goal, nps.fastMovementSpeed);
                nps.playerButton.transform.DOScale(Vector3.one, nps.fastMovementSpeed);
            }
            else
            {
                nps.playerParent.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * (size + 1), 340);
            }
        }


        GUIManagerScript.SetBackButton(false);
    }

    void Update()
    {
        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();

        Assert.IsNotNull<GameManagerScript>(gameManager);

        if (gameManager.IsInPostGame())
        {
            sliderText.text = "";

            infoText.enabled = false;
            countdownText.enabled = false;

            int song = networkedPScript.GetSongID();
            int match = networkedPScript.GetMatchSongID();
            if (song == match)
            {
                answerText.text = "Correct!";
            }
            else
            {
                answerText.text = "Wrong!";
            }
            detailsText.text = "Scored this round:\n+" + networkedPScript.GetScoredThisRound().ToString() + "\nAuto advancing in " + Mathf.CeilToInt(gameManager.endgameCountDown);
        }
        else
        {
            float countDown = gameManager.countDown;

            float captainsCountdown = networkedPScript.captainsCountdown;
            if (captainsCountdown > 0)
            {
                sliderText.text = "";
                infoText.enabled = true;

                if (captainsCountdown < 1)
                {
                    infoText.text = "DANCE!";
                }
                else if (captainsCountdown >= (4f)) //Plus one second for the "Dance" end 
                {
                    infoText.text = "Ready?";
                }
                else
                {
                    infoText.text = "" + Mathf.Floor(captainsCountdown);
                }
            }
            else if (countDown > 0)
            {
                bool chosen = (networkedPScript.GetMatchSongID() == -1);
                if (chosen)
                    sliderText.text = "Chose your dance partner:";
                else
                    sliderText.text = "Press back to undo current choice";
                infoText.enabled = false;
                countdownText.enabled = true;
                countdownText.text = "" + Mathf.Ceil(countDown);

                if (reminded == false)
                {
                    if (countDown < 30 && chosen)
                    {
                        Debug.Log("Reminder to pick!");
                        AudioManagerScript.instance.PlayFind();
                        reminded = true;
                    }
                }
            }
            else
            {
                sliderText.text = "Other players:";
                infoText.enabled = false;
            }
        }
    }
}
