using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using App.Networking;
using App.Data;
using App.Audio;

namespace App.Controllers
{
    public class PostGameManager : MonoBehaviour
    {

        [Header("UI Element References")]
        [SerializeField]
        private TextMeshProUGUI correctText;
        [SerializeField]
        private TextMeshProUGUI songOneText;
        [SerializeField]
        private RectTransform contentChosen;
        [SerializeField]
        private RectTransform contentActual;

        [SerializeField]
        private Button lobbyReturnButton;
        [SerializeField]
        private Image playerReadyImage;

        [SerializeField, Header("Prefab References")]
        private ChoosePlayerButton selectablePlayerButton;

        private Dictionary<PlayerDataSnapShot, ChoosePlayerButton> chosenPlayerIcons = new Dictionary<PlayerDataSnapShot, ChoosePlayerButton>();
        //private Dictionary<PlayerDataSnapShot, ChoosePlayerButton> actualPlayerIcons = new Dictionary<PlayerDataSnapShot, ChoosePlayerButton>();

        public void PlayerReady()
        {
            // check local player ready for another round!
            MainController.s_Instance.GameController.LocalPlayer.CmdSetReady();
        }

        /// <summary>
        /// Only the server client will be able to call this.
        /// </summary>
        public void ReturnToLobby()
        {
            MainController.s_Instance.GameController.CmdCompleteGame();
        }

        /// <summary>
        /// Listen for player ready events.
        /// </summary>
        /// <param name="player"></param>
        private void OnPlayerReadyAction(DanceTogetherPlayer player)
        {
            if (player.IsReady)
            {
                playerReadyImage.enabled = true;
            } else
            {
                playerReadyImage.enabled = false;
            }
        }

        /// <summary>
        /// When the View is opened, this will gather its info.
        /// Post game view should only be called in post game state, or enable could fail to do its job.
        /// </summary>
        private void OnEnable()
        {
            if (!MainController.s_InstanceExists)
                return;

            // Check controller status.
            if (MainController.s_Instance.GameController == null)
                return;

            // Check local player status.
            if (MainController.s_Instance.GameController.LocalPlayer == null)
                return;

            // Check server status.
            if (MainController.s_Instance.GameController.LocalPlayer.isServer)
            {
                lobbyReturnButton.gameObject.SetActive(true);
            }
            else
            {
                lobbyReturnButton.gameObject.SetActive(false);
            }

            playerReadyImage.enabled = false;

            // Check player snapshots
            if (NetworkController.s_Instance.LocalPlayer == null)
            {
                Debug.LogWarning("Data has failed to pass");
                return;
            }

            List<PlayerDataSnapShot> correctPlayers = MainController.s_Instance.GameController.GetAllPlayersWithSongId(NetworkController.s_Instance.LocalPlayer.SongID, true); // attempt to fine correct player
            if (correctPlayers.Count < 1)
            {
                Debug.LogWarning("Data has failed to pass - count was : " + correctPlayers.Count);
                return;
            }

            // for now we will just check the first player in the selected list. TODO support for multiple selections.
            PlayerDataSnapShot firstSelectedPlayer = NetworkController.s_Instance.LocalPlayer.SelectedPlayers[0];

            // for now we will just grab the first player in the group list
            PlayerDataSnapShot firstCorrectPlayer = correctPlayers[0];
            // is correct?
            if (NetworkController.s_Instance.LocalPlayer.SongID == firstSelectedPlayer.SongID)
            {
                correctText.text = "Correct!";
                correctText.color = Color.green;
                DanceTogetherAudioManager.s_Instance.PlayCorrectSFX();
            } else
            {
                correctText.text = "Incorrect";
                correctText.color = Color.red;
                DanceTogetherAudioManager.s_Instance.PlayWrongSFX();
            }

            // song titles.
            songOneText.text = "You heard: " + MainController.s_Instance.GameController.AvailableMusicList[MainController.s_Instance.GameController.currentGenreIndex].MusicTrackList[MainController.s_Instance.GameController.LocalPlayer.SongID].TrackName; // local player song

            // Create new chosen player Icons.
            foreach (PlayerDataSnapShot playerSS in NetworkController.s_Instance.LocalPlayer.SelectedPlayers)
            {
                ChoosePlayerButton newButton = Instantiate(selectablePlayerButton, contentChosen);
                newButton.UpdateButtonInfo(playerSS, false);
            }

            // Create new actual player Icons.
            foreach (PlayerDataSnapShot playerSS in correctPlayers)
            {
                ChoosePlayerButton newButton = Instantiate(selectablePlayerButton, contentActual);
                newButton.UpdateButtonInfo(playerSS, false);
            }

            // set delegates
            NetworkController.s_Instance.LocalPlayer.playerReadyEvent += OnPlayerReadyAction;
        }

        private void OnDisable()
        {
            correctText.text = "Answer?";
            correctText.color = Color.white;

            songOneText.text = "Song Title 1";

            // remove icons
            foreach(RectTransform child in contentActual)
            {
                Destroy(child.gameObject);
            }

            foreach(RectTransform child in contentChosen)
            {
                Destroy(child.gameObject);
            }

            // remove delegates
            if (!NetworkController.s_InstanceExists)
                return; 

            if (NetworkController.s_Instance.LocalPlayer != null)
            {
                NetworkController.s_Instance.LocalPlayer.playerReadyEvent -= OnPlayerReadyAction;
            }
        }
    }
}
