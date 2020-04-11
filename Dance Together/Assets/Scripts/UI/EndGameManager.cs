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

        /// <summary>
        /// Call Populate List from UI button.
        /// </summary>
        public void PopulateList()
        {
            ClearButtons();
            foreach(PlayerDataSnapShot playerSS in MainController.s_Instance.GameController.ActivePlayerData)
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
            MainController.s_Instance.GameController.LocalGotoPostGame();
        }

        public void ChoosePlayer(PlayerDataSnapShot player)
        {
            // set local view
            //Debug.Log("local player ? : " + controller.LocalPlayer.PlayerID);
            MainController.s_Instance.GameController.LocalPlayer.CmdSetSongMatchID(player.SongID);
            continueButton.interactable = true;
            // close popup
            PopupView.CloseView();
            // set choose player button to match selection
            choosenColorImage.color = player.PlayerColor.Color;
            choosenPlayerID.text = player.PlayerID.ToString();
        }

        private void OnEnable()
        {
            if(MainController.s_Instance.GameController.LocalPlayer == null)
            {
                Debug.LogWarning("Local Player is null");
                return;
            }
            playerColor.color = MainController.s_Instance.GameController.LocalPlayer.playerColor.Color;
            playerID.text = MainController.s_Instance.GameController.LocalPlayer.PlayerID.ToString();
            if (MainController.s_Instance.GameController.ActivePlayerData.Count > 1)
                continueButton.interactable = false; // default behaviour.
            else
                continueButton.interactable = true; // for testing app with single player.

            // reset the choose player button
            choosenColorImage.color = Color.white;
            choosenPlayerID.text = "Tap to Choose Player";
        }
    }
}

