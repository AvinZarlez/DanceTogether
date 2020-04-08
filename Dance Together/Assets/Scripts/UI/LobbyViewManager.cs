using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using App.Networking;
using App.Audio;

namespace App.Controllers {
    public class LobbyViewManager : MonoBehaviour
    {
        // controller reference
        private NetworkController networkController;

        // private VARs set in inspector.
        [Header("Prefab References")]
        [SerializeField]
        private LocalPlayerIcon localPlayerIcon;
        [SerializeField]
        private LobbyPlayerIcon remotePlayerIcon;

        [Header("Display Container")]
        [SerializeField]
        private RectTransform iconDisplayParent;

        [Header("Button References")]
        [SerializeField]
        private Button readyButton;
        [SerializeField]
        private Button genreButton;

        [Header("Text References")]
        [SerializeField]
        private TextMeshProUGUI genreText;

        //private internal VARs
        private Dictionary<DanceTogetherPlayer, LobbyPlayerIcon> lobbyIcons = new Dictionary<DanceTogetherPlayer, LobbyPlayerIcon>();

        public RectTransform IconDisplayParent
        {
            get { return iconDisplayParent; }
        }
        
        public void AddNewLobbyIcon(DanceTogetherPlayer _player)
        {
            if (_player == null) // check if player is null
                return;

            //if (_player.isLocalPlayer) // check if player is remote.
                //return;

            if (!lobbyIcons.ContainsKey(_player)) // if lobby icon does not exist.
            {
                LobbyPlayerIcon newIcon = Instantiate(remotePlayerIcon, iconDisplayParent);
                newIcon.Init(_player);
                _player.syncVarsChangedEvent += UpdateIcons; // add delegate callback to refresh icons.
                lobbyIcons.Add(_player, newIcon);
            }

            if (_player.isLocalPlayer)
            {
                localPlayerIcon.UpdateAll(_player.playerColor, _player.PlayerID);
            }
            
        }

        public void RemoveLobbyIcon(DanceTogetherPlayer _player)
        {
            if (_player == null) // check if player is null
                return;

            if (lobbyIcons.ContainsKey(_player)) // if lobby icon exists.
            {
                _player.syncVarsChangedEvent -= UpdateIcons; // remove delegate reference
                Destroy(lobbyIcons[_player].gameObject); // destroy lobby icon
                lobbyIcons.Remove(_player); // remove dictionary kvp.
            }

        }

        public void Init(NetworkController _networkController)
        {
            networkController = _networkController;

            networkController.Controller.GameController.MusicGenreChanged += UpdateGenre;

            UpdateGenre(networkController.Controller.GameController.GetCurrentTrackList().Genre); // init the genre field
        }

        /// <summary>
        ///  Attach Update Icons to syncVarsChanged of dancetogetherplayer
        /// </summary>
        /// <param name="_player"></param>
        private void UpdateIcons(DanceTogetherPlayer _player)
        {
            if(_player.isLocalPlayer)
            {
                localPlayerIcon.UpdateAll(_player.playerColor, _player.PlayerID);
                readyButton.interactable = !_player.IsReady; // if ready, dont hit that button anymore : maybe this is harsh?
            }

            if (lobbyIcons.ContainsKey(_player))
            {
                lobbyIcons[_player].UpdateAll(_player.playerColor.Color, _player.PlayerID, _player.IsReady);
            }
        }

        private void UpdateGenre(SongGenre _genre)
        {
            genreText.text = "Genre: " + _genre.name;
        }

        public void RefreshIcons()
        {
            foreach(var kvp in lobbyIcons)
            {
                UpdateIcons(kvp.Key);
            }
        }

        private void OnEnable()
        {
            readyButton.interactable = true;
            RefreshIcons();

            // if the client is the server, allow user to change the genre.
            genreButton.interactable = networkController.LocalPlayer.isServer;
        }
        private void OnDestroy()
        {
            foreach(var kvp in lobbyIcons)
            {
                kvp.Key.syncVarsChangedEvent -= UpdateIcons;
            }

            networkController.Controller.GameController.MusicGenreChanged -= UpdateGenre;
        }

        /// <summary>
        /// Commands local network player to set ready
        /// </summary>
        public void TogglePlayerReady()
        {
            if(networkController?.LocalPlayer != null)
            {
                networkController.LocalPlayer.CmdSetReady();
            }
        }
        /// <summary>
        /// Commands Game Controller to change genre if possible.
        /// </summary>
        public void ChangeGenre()
        {
            networkController.Controller.GameController.CmdChangeMusicGenre();
        }
    }
}
