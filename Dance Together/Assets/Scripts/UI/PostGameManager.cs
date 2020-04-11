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
            PlayerDataSnapShot otherPlayer = MainController.s_Instance.GameController.OtherPlayerWithSongID(MainController.s_Instance.GameController.LocalPlayer.SelectedMatchSongId); // attempt to find and return the first player with same song.
            if (otherPlayer == null)
            {
                Debug.LogWarning("Data has failed to pass");
                return;
            }

            PlayerDataSnapShot correctPlayer = MainController.s_Instance.GameController.OtherPlayerWithSongID(MainController.s_Instance.GameController.LocalPlayer.SongID); // attempt to fine correct player
            if (correctPlayer == null)
            {
                Debug.LogWarning("Data has failed to pass");
                return;
            }

            // is correct?
            if (MainController.s_Instance.GameController.LocalPlayer.CheckAnsweredCorrect())
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
            songTwoText.text = "Your choice heard: " + MainController.s_Instance.GameController.AvailableMusicList[MainController.s_Instance.GameController.currentGenreIndex].MusicTrackList[MainController.s_Instance.GameController.LocalPlayer.SelectedMatchSongId].TrackName; // song of partner you chose.

            partnerColorImage.color = otherPlayer.PlayerColor.Color;
            partnerIdText.text = otherPlayer.PlayerID.ToString();

            partnerColorCorrectImage.color = correctPlayer.PlayerColor.Color;
            partnerIdCorrectText.text = correctPlayer.PlayerID.ToString();

            // set delegates
            NetworkController.s_Instance.LocalPlayer.playerReadyEvent += OnPlayerReadyAction;
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
            if (NetworkController.s_Instance.LocalPlayer != null)
            {
                NetworkController.s_Instance.LocalPlayer.playerReadyEvent -= OnPlayerReadyAction;
            }
        }
    }
}
