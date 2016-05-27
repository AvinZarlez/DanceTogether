using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LocalPlayerScript : MonoBehaviour
{
    // To make referencing easier/less calls.
    //private PlayerParentScript playerParentScript;
    private NetworkedPlayerScript networkedPScript;
    private Text countdownText; // UI text object named "UI_Countdown"
    private Text infoText; // UI text object named "UI_InfoText"
    private Text detailsText; // UI text object named "UI_DetailsText"

    void Start()
    {
        //playerParentScript = GameObject.FindWithTag("PlayerParent").GetComponent<PlayerParentScript>();

        networkedPScript = GetComponent<NetworkedPlayerScript>();

        GameObject obj1 = GameObject.Find("UI_Countdown");
        countdownText = obj1.GetComponent<Text>();
        countdownText.enabled = false;

        GameObject obj2 = GameObject.Find("UI_InfoText");
        infoText = obj2.GetComponent<Text>();
        infoText.enabled = false;

        GameObject obj3 = GameObject.Find("UI_DetailsText");
        detailsText = obj3.GetComponent<Text>();
        detailsText.enabled = false;

        Application.runInBackground = true;
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    
    public void BackButtonPressed()
    {
        // Nothing happens
    }

    void Update()
    {

        if (GameManagerScript.instance.IsInPostGame())
        {
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
            detailsText.text = "Current Score: " + networkedPScript.GetScore().ToString();
        }
        else
        {
            detailsText.enabled = false;

            float countDown = GameManagerScript.instance.countDown;

            float captainsCountdown = networkedPScript.captainsCountdown;
            if (captainsCountdown > 0)
            {
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
                infoText.enabled = false;
                countdownText.enabled = true;
                countdownText.text = "" + Mathf.Ceil(countDown);
            }
            else
            {
                infoText.enabled = false;
            }
        }
    }
}
