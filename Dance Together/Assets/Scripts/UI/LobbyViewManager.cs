using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using App.Networking;
using App.Audio;

namespace App.Controllers {
    public class LobbyViewManager : MonoBehaviour
    {
        // controller reference
        //private NetworkController networkController;

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

        private NetworkController networkController;
        private MainController mainController;

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
            Debug.Log("Called add icon");
        }

        public void RemoveLobbyIcon(DanceTogetherPlayer _player)
        {
            if (_player == null) // check if player is null
                return;

            if (lobbyIcons.ContainsKey(_player)) // if lobby icon exists.
            {
                _player.syncVarsChangedEvent -= UpdateIcons; // remove delegate reference
                if (lobbyIcons[_player].gameObject != null) // prevents error on app shutdown.
                {
                    Destroy(lobbyIcons[_player].gameObject); // destroy lobby icon
                }
                lobbyIcons.Remove(_player); // remove dictionary kvp.
            }
        }

        private void Start()
        {
            if (NetworkController.s_InstanceExists)
            {
                networkController = NetworkController.s_Instance;

                networkController.PlayerRegisteredEvent += OnPlayerRegisteredAction;
                networkController.PlayerUnRegisteredEvent += OnPlayerUnRegisteredAction;
                networkController.NetworkManager.clientDisconnected += OnClientDisconnectedAction;
            }

            if (MainController.s_InstanceExists)
            {
                mainController = MainController.s_Instance;

                mainController.GameController.MusicGenreChanged += UpdateGenre; // attach to genre update event
            }

            UpdateGenre(MainController.s_Instance.GameController.GetCurrentTrackList().Genre); // refresh current interface.
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

            genreButton.interactable = NetworkController.s_Instance.LocalPlayer.isServer; // allow user to use the genre button. they are the server.
        }

        private void UpdateGenre(SongGenre _genre)
        {
            if(_genre != null)
                genreText.text = "Genre: " + _genre.name;
        }

        private void OnPlayerRegisteredAction(DanceTogetherPlayer _player)
        {
            AddNewLobbyIcon(_player);
        }
        private void OnPlayerUnRegisteredAction(DanceTogetherPlayer _player)
        {
            RemoveLobbyIcon(_player);
        }
        private void OnClientDisconnectedAction(NetworkConnection conn)
        {
            Reset();
        }

        public void RefreshIcons()
        {
            foreach(var kvp in lobbyIcons)
            {
                UpdateIcons(kvp.Key);
            }
        }

        private void OnDestroy()
        {
            foreach (var kvp in lobbyIcons)
            {
                kvp.Key.syncVarsChangedEvent -= UpdateIcons;
            }

            if(networkController != null)
            {
                networkController.PlayerRegisteredEvent -= OnPlayerRegisteredAction;
                networkController.PlayerUnRegisteredEvent -= OnPlayerUnRegisteredAction;
                networkController.NetworkManager.clientDisconnected -= OnClientDisconnectedAction;
            }

            if (mainController != null)
            {
                if (mainController.GameController != null)
                {
                    mainController.GameController.MusicGenreChanged -= UpdateGenre;
                }
            }
        }

        /// <summary>
        /// Commands local network player to set ready
        /// </summary>
        public void TogglePlayerReady()
        {
            if(NetworkController.s_Instance.LocalPlayer != null)
            {
                NetworkController.s_Instance.LocalPlayer.CmdSetReady();
            }
        }
        /// <summary>
        /// Commands Game Controller to change genre if possible.
        /// </summary>
        public void ChangeGenre()
        {
            MainController.s_Instance.GameController.CmdChangeMusicGenre();
        }

        public void Reset()
        {
            readyButton.interactable = true;
        }

        
    }
}
