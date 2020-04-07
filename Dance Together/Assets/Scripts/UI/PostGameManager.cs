using UnityEngine;
using TMPro;
using UnityEngine.UI;
using App.Networking;
using App.Data;

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
        private TextMeshProUGUI songTwoText;

        [SerializeField]
        private Image partnerColorImage;
        [SerializeField]
        private TextMeshProUGUI partnerIdText;
        [SerializeField]
        private Image partnerColorCorrectImage;
        [SerializeField]
        private TextMeshProUGUI partnerIdCorrectText;
        [SerializeField]
        private Button lobbyReturnButton;
        [SerializeField]
        private Image playerReadyImage;


        private DanceTogetherGameManager controller;

        public void Init(DanceTogetherGameManager _controller)
        {
            controller = _controller;

            if (controller == null)
            {
                Debug.LogWarning("PostGameManager could not be Initialized. controller reference is null.");
                return;
            }
        }

        public void PlayerReady()
        {
            // check local player ready for another round!
            controller.LocalPlayer.CmdSetReady();
        }

        /// <summary>
        /// Only the server client will be able to call this.
        /// </summary>
        public void ReturnToLobby()
        {
            controller.CmdCompleteGame();
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
            // Check controller status.
            if (controller == null)
                return;

            // Check local player status.
            if (controller.LocalPlayer == null)
                return;

            // Check server status.
            if (controller.isServer)
            {
                lobbyReturnButton.gameObject.SetActive(true);
            }
            else
            {
                lobbyReturnButton.gameObject.SetActive(false);
            }

            playerReadyImage.enabled = false;

            // Check player snapshots
            PlayerDataSnapShot otherPlayer = controller.OtherPlayerWithSongID(controller.LocalPlayer.SelectedMatchSongId); // attempt to find and return the first player with same song.
            if (otherPlayer == null)
            {
                Debug.LogWarning("Data has failed to pass");
                return;
            }

            PlayerDataSnapShot correctPlayer = controller.OtherPlayerWithSongID(controller.LocalPlayer.SongID); // attempt to fine correct player
            if (correctPlayer == null)
            {
                Debug.LogWarning("Data has failed to pass");
                return;
            }

            // is correct?
            if (controller.LocalPlayer.CheckAnsweredCorrect())
            {
                correctText.text = "Correct!";
                correctText.color = Color.green;
                controller.Controller.AudioController.PlayCorrectSFX();
            } else
            {
                correctText.text = "Incorrect";
                correctText.color = Color.red;
                controller.Controller.AudioController.PlayWrongSFX();
            }

            // song titles.
            songOneText.text = "You heard: " + controller.AvailableMusicList[controller.currentGenreIndex].MusicTrackList[controller.LocalPlayer.SongID].TrackName; // local player song
            songTwoText.text = "Your choice heard: " + controller.AvailableMusicList[controller.currentGenreIndex].MusicTrackList[controller.LocalPlayer.SelectedMatchSongId].TrackName; // song of partner you chose.

            partnerColorImage.color = otherPlayer.PlayerColor.Color;
            partnerIdText.text = otherPlayer.PlayerID.ToString();

            partnerColorCorrectImage.color = correctPlayer.PlayerColor.Color;
            partnerIdCorrectText.text = correctPlayer.PlayerID.ToString();

            // set delegates
            controller.LocalPlayer.playerReadyEvent += OnPlayerReadyAction;
        }

        private void OnDisable()
        {
            correctText.text = "Answer?";
            correctText.color = Color.white;

            songOneText.text = "Song Title 1";
            songTwoText.text = "Song Title 2";

            partnerColorImage.color = Color.white;
            partnerIdText.text = "0";

            partnerColorCorrectImage.color = Color.white;
            partnerIdCorrectText.text = "0";

            // remove delegates
            controller.LocalPlayer.playerReadyEvent -= OnPlayerReadyAction;
        }
    }
}
