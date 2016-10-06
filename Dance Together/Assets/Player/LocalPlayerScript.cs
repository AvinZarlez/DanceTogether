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
    private Text infoText; // UI text object named "UI_InfoText"
    private Text detailsText; // UI text object named "UI_DetailsText"
    private Text sliderText; // UI text object named "UI_SliderText"

    void Start()
    {
        //playerParentScript = GameObject.FindWithTag("PlayerParent").GetComponent<PlayerParentScript>();

        networkedPScript = GetComponent<NetworkedPlayerScript>();
        
        countdownText = GUIManagerScript.countdownText;

        sliderText = GUIManagerScript.sliderText;

        infoText = GUIManagerScript.infoText;

        detailsText = GUIManagerScript.detailsText;

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
                RectTransform rt = nps.playerParent.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(160 * (size + 1), 340);
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
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

            countdownText.enabled = false;
            infoText.enabled = true; // Redundent?
            detailsText.enabled = true;

            int song = networkedPScript.GetSongID();
            int match = networkedPScript.GetMatchSongID();
            if (song == match)
            {
                infoText.text = "Correct!";
            }
            else
            {
                infoText.text = "Wrong!";
            }
            detailsText.text = "Scored this round:\n+" + networkedPScript.GetScoredThisRound().ToString();
        }
        else
        {
            detailsText.enabled = false;

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
                if (networkedPScript.GetMatchSongID() == -1)
                    sliderText.text = "Chose your dance partner:";
                else
                    sliderText.text = "Press back to undo current choice";
                infoText.enabled = false;
                countdownText.enabled = true;
                countdownText.text = "" + Mathf.Ceil(countDown);
            }
            else
            {
                sliderText.text = "Other players:";
                infoText.enabled = false;
            }
        }
    }
}
