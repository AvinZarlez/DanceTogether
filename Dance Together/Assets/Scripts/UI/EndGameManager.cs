using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using App.Networking;
using App.Data;


namespace App.Controllers
{
    public class EndGameManager : MonoBehaviour
    {
        private DanceTogetherGameManager controller;

        [Header("Button Prefab")]
        [SerializeField]
        private ChoosePlayerButton buttonPrefab;

        [Header("Managed GUI Elements")]
        [SerializeField]
        private Image playerColor;
        [SerializeField]
        private TextMeshProUGUI playerID;
        [SerializeField]
        private Button continueButton;
        [SerializeField]
        private Image choosenColorImage;
        [SerializeField]
        private TextMeshProUGUI choosenPlayerID;
        [Header("Player List references")]
        [SerializeField]
        private RectTransform playerList; // rect parent to put button in. If I Fits, I Sits.
        [SerializeField]
        private View PopupView; // popup view to open and close to show player selection options

        private List<ChoosePlayerButton> playerButtons = new List<ChoosePlayerButton>();


        public void Init(DanceTogetherGameManager _controller)
        {
            controller = _controller;

            if(controller == null)
            {
                Debug.Log("Controller was assigned null.");
                return;
            }
        }

        /// <summary>
        /// Call Populate List from UI button.
        /// </summary>
        public void PopulateList()
        {
            ClearButtons();
            foreach(PlayerDataSnapShot playerSS in controller.ActivePlayerData)
            {
                // dont add ourselves to the choosable list.
                if (!playerSS.IsLocalPlayer)
                {
                    ChoosePlayerButton newButton = Instantiate(buttonPrefab, playerList);
                    newButton.UpdateButtonInfo(this, playerSS);
                    playerButtons.Add(newButton);
                }
            }

            PopupView.OpenView();
        }

        public void ClearButtons()
        {
            foreach (ChoosePlayerButton button in playerButtons)
            {
                Destroy(button.gameObject);
            }
            playerButtons.Clear();
        }

        /// <summary>
        /// Call continue from UI button
        /// </summary>
        public void Continue()
        {
            controller.LocalGotoPostGame();
        }

        public void ChoosePlayer(PlayerDataSnapShot player)
        {
            // set local view
            //Debug.Log("local player ? : " + controller.LocalPlayer.PlayerID);
            controller.LocalPlayer.CmdSetSongMatchID(player.SongID);
            continueButton.interactable = true;
            // close popup
            PopupView.CloseView();
            // set choose player button to match selection
            choosenColorImage.color = player.PlayerColor.Color;
            choosenPlayerID.text = player.PlayerID.ToString();
        }

        private void OnEnable()
        {
            if(controller.LocalPlayer == null)
            {
                Debug.LogWarning("Local Player is null");
                return;
            }
            playerColor.color = controller.LocalPlayer.playerColor.Color;
            playerID.text = controller.LocalPlayer.PlayerID.ToString();
            if (controller.ActivePlayerData.Count > 1)
                continueButton.interactable = false; // default behaviour.
            else
                continueButton.interactable = true; // for testing app with single player.

            // reset the choose player button
            choosenColorImage.color = Color.white;
            choosenPlayerID.text = "Tap to Choose Player";
        }
    }
}

