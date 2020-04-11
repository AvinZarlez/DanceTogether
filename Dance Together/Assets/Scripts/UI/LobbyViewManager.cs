using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
        private DanceTogetherGameManager gameController;

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

        private void Awake()
        {
            if (NetworkController.s_InstanceExists)
            {
                networkController = NetworkController.s_Instance;

                networkController.PlayerRegisteredEvent += OnPlayerRegisteredEvent;
                networkController.PlayerUnRegisteredEvent += OnPlayerUnRegisteredEvent;
            }
            if (MainController.s_Instance.GameController != null)
            {
                gameController = MainController.s_Instance.GameController;

                gameController.MusicGenreChanged += UpdateGenre; // attach to genre update event
                UpdateGenre(gameController.GetCurrentTrackList().Genre); // refresh current interface.
            }
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

                //genreButton.interactable = _player.isServer; // allow user to use the genre button. they are the server.
            }

            if (lobbyIcons.ContainsKey(_player))
            {
                lobbyIcons[_player].UpdateAll(_player.playerColor.Color, _player.PlayerID, _player.IsReady);
            }

            genreButton.interactable = networkController.LocalPlayer.isServer; // allow user to use the genre button. they are the server.
        }

        private void UpdateGenre(SongGenre _genre)
        {
            genreText.text = "Genre: " + _genre.name;
        }

        private void OnPlayerRegisteredEvent(DanceTogetherPlayer _player)
        {
            AddNewLobbyIcon(_player);
        }
        private void OnPlayerUnRegisteredEvent(DanceTogetherPlayer _player)
        {
            RemoveLobbyIcon(_player);
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
            StopAllCoroutines();
            StartCoroutine(DelayedInit());
        }
        private void OnDestroy()
        {
            foreach(var kvp in lobbyIcons)
            {
                kvp.Key.syncVarsChangedEvent -= UpdateIcons;
            }

            if (gameController != null)
            {
                gameController.MusicGenreChanged -= UpdateGenre;
            }
        }

        /// <summary>
        /// Commands local network player to set ready
        /// </summary>
        public void TogglePlayerReady()
        {
            if(networkController.LocalPlayer != null)
            {
                networkController.LocalPlayer.CmdSetReady();
            }
        }
        /// <summary>
        /// Commands Game Controller to change genre if possible.
        /// </summary>
        public void ChangeGenre()
        {
            gameController.CmdChangeMusicGenre();
        }

        private IEnumerator DelayedInit()
        {
            yield return new WaitForSeconds(0.5f);
            readyButton.interactable = true;
            RefreshIcons();
            UpdateGenre(MainController.s_Instance.GameController.GetCurrentGenre());
            yield return null;
        }
    }
}
