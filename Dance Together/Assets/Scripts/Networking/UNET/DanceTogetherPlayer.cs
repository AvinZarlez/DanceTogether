using System;
using UnityEngine;
using UnityEngine.Networking;
using App.Utility;
using App.Data;

namespace App.Networking {
    [RequireComponent(typeof(NetworkIdentity))]
    public class DanceTogetherPlayer : NetworkBehaviour
    {
        // Delegate Events
        public event Action<DanceTogetherPlayer> syncVarsChangedEvent;
        public event Action<DanceTogetherPlayer> playerReadyEvent;
        public event Action<DanceTogetherPlayer> matchSongIDChosenEvent;

        // Network controller reference
        private NetworkController controller;

        private const string versionNum = "0.0.6";

        // sync Vars
        [SerializeField, SyncVar(hook = "OnMyIdChange")]
        private int playerID = 0;
        [SyncVar(hook = "OnMyColor")]
        public IndexedColor playerColor = DanceTogetherUtility.GetIndexColor(0);
        [SerializeField, SyncVar(hook = "OnIsReady")]
        private bool isReady = false;
        [SerializeField ,SyncVar(hook = "OnSongId")]
        private int songID = -1;
        [SerializeField, SyncVar]
        private int selectedMatchSongId = -1; // The other player this player has picked as a match
        /// <summary>
        /// isActivePlayer is meant to be set by the Game Manager to track actively participating players. 
        /// Players who join the lobby after the game has started are marked to not count in game mechanics.
        /// inactive players should wait in lobby until game has finished.
        /// </summary>
        [SerializeField, SyncVar]
        private bool isActivePlayer = false; // Mark the player Active if actively taking part in current game.

        /// <summary>
        /// Here is a player snap shot that may be passed, the snap shot will be used for basic data.
        /// The SnapShot should persist if the player has disconnected.
        /// </summary>
        private PlayerDataSnapShot playerSnapShot = new PlayerDataSnapShot();
        public PlayerDataSnapShot PlayerSnapShot
        {
            get { return playerSnapShot; }
        }

        /// <summary>
        /// Check if the song id selected is the same as the song id given.
        /// Note: if either song id or chosen id is left -1, bool will return false;
        /// </summary>
        public bool CheckAnsweredCorrect()
        {
            if (songID == -1 || selectedMatchSongId == -1)
                return false;

            if (songID == selectedMatchSongId)
                return true;

            return false;
        }

        #region SyncVar Hooks
        // Sync Var Hooks
        private void OnMyIdChange(int _newID)
        {
            playerID = _newID;

            playerSnapShot.UpdateSS(this);

            if (syncVarsChangedEvent != null)
            {
                syncVarsChangedEvent(this);
            }
        }

        private void OnMyColor(IndexedColor _newColor)
        {
            playerColor = _newColor;

            playerSnapShot.UpdateSS(this);

            if (syncVarsChangedEvent != null)
            {
                syncVarsChangedEvent(this);
            }
        }

        private void OnSongId(int _value)
        {
            songID = _value;

            playerSnapShot.UpdateSS(this);

            if (syncVarsChangedEvent != null)
            {
                syncVarsChangedEvent(this);
            }
        }

        private void OnIsReady(bool _value)
        {
            isReady = _value;

            if (syncVarsChangedEvent != null)
            {
                syncVarsChangedEvent(this);
            }
            if (playerReadyEvent != null)
            {
                playerReadyEvent(this);
            }
        }
        #endregion

        #region Public GETS
        public int PlayerID
        {
            get { return playerID; }
            set
            {
                playerID = value; // set new value
                playerColor = DanceTogetherUtility.GetIndexColor(value); // assign color based on id.
            }
        }
        public bool IsReady
        {
            get { return isReady; }
        }
        public bool IsActivePlayer
        {
            get { return isActivePlayer; }
        }
        public int SongID
        {
            get { return songID; }
        }
        public int SelectedMatchSongId
        {
            get { return selectedMatchSongId; }
        }
        #endregion

        #region Monobehavior Methods
        private void Start()
        {
            Init();
        }

        private void OnDestroy()
        {
            controller?.UnRegisterPlayer(this);

            syncVarsChangedEvent = null;
            playerReadyEvent = null;
            matchSongIDChosenEvent = null;

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Init is called when the network manager instantiates the player.
        /// </summary>
        public void Init()
        {
            controller = FindObjectOfType<NetworkController>();

            if (controller == null)
            {
                Debug.LogWarning("Dance Together Player has been Initialized with a null NetworkController. - Please check Controller links");
                return;
            }

            controller.RegisterNewPlayer(this); // register player with network controller.
            playerSnapShot.UpdateSS(this); // update the Snapshot
        }

        /// <summary>
        /// local active only.
        /// </summary>
        [Command]
        public void CmdSetActive(bool _value)
        {
            isActivePlayer = _value;
        }
        [Command]
        public void CmdSetSongID(int value)
        {
            songID = value;
        }
        [Command]
        public void CmdSetSongMatchID(int value)
        {
            selectedMatchSongId = value;
        }
        [Command]
        public void CmdSetReady()
        {
            isReady = true; ;
        }
        [Command]
        public void CmdClearReady()
        {
            isReady = false;
        }
        [Command]
        public void CmdResetPlayer()
        {
            songID = -1;
            selectedMatchSongId = -1;
            isReady = false;
            isActivePlayer = false;
        }
        #endregion
        
    }
}