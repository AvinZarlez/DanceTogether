using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using App.Data;
using App.Events;
using App.Networking;
using App.Audio;
using App.Utility;

namespace App.Controllers
{
    public class MainController : Singleton<MainController>
    {
        // private VARS
        // inspector visible Vars
        [Header("Game Settings")]
        [SerializeField]
        private GameType currentGameType; // this might make more sense in Game Controller.

        /// <summary>
        /// GameState Event Passed by Manager
        /// </summary>
        [Header("Game Events")]
        [SerializeField]
        private GameEvent gameStateEvent;
        /// <summary>
        /// Error Event fired by main manager when disconnected by fault
        /// </summary>
        [SerializeField]
        private GameEvent errorEvent;

        [SerializeField, Header("Game Controller Reference")]
        private DanceTogetherGameManager gameController;
        private NetworkController networkController;
        private DanceTogetherAudioManager audioController;

        // gameObject references.
        [Header("Controller Linkage")]

        [Header("Dev Options")]
        public bool verboseLogging = false; // use to get deep messaging in logs about networking.

        // hidden Vars
        [SerializeField]
        private GameEventPayLoad.States currentGameState;

        public GameType CurrentGameType
        {
            get { return currentGameType; }
        }

        public DanceTogetherGameManager GameController
        {
            get { return gameController; }
        }
        
        protected override void Awake()
        {
            // clean Game Event Scriptable Objects.
            gameStateEvent?.Data.Clear();
            errorEvent?.Data.Clear();

            Initialize();
            base.Awake();
        }

        public void Initialize()
        {
            StopAllCoroutines();
            StartCoroutine(InitSequence());
        }

        // Public functionality
        public void AutoConnect()
        {
            NetworkController.s_Instance.AutoJoinGame();
            //networkManager.minPlayers = currentGameType.Data.MinPlayers;
            //networkManager.AutoConnect();
            //lobbyEnterEvent?.Raise();
            //commands.JoinedLobbyEvent?.Raise();
        }
        public void CreateGame()
        {
            // start game as host.
            networkController?.StartNewLanGame();
        }

        public void JoinGame()
        {
            // browse games
            networkController?.StartSearching();
            SetGameState(GameEventPayLoad.States.SearchingForGame);
        }

        public void GoToMainMenu()
        {
            // leave any game and return to main menu
            StopAllCoroutines();
            SetGameState(GameEventPayLoad.States.MainMenu);
            networkController?.Reset();
            gameController?.Reset();
            audioController?.Reset();
        }

        private void SetGameState(GameEventPayLoad.States state)
        {
            // change Game State
            currentGameState = state;
            // send Game State Change Message.
            gameStateEvent?.Raise(currentGameState);

            Debug.Log("GameState : " + currentGameState);
        }

        #region Delegate Fired Events
        private void OnClientConnectedAction(NetworkConnection conn)
        {
            SetGameState(GameEventPayLoad.States.Lobby);
            audioController?.BeginLobbyMusic();
        }
        private void OnClientDisconnectedAction(NetworkConnection conn)
        {
            GoToMainMenu();

            if (conn.lastError != NetworkError.Ok)
            {
                errorEvent?.Raise("Connection Ended : " + conn.lastError.ToString());
            }
        }
        #endregion

        #region CallBacks 
        private void OnGameInitialized()
        {
            SetGameState(GameEventPayLoad.States.GameInitialize);
        }
        private void OnGameBegin()
        {
            SetGameState(GameEventPayLoad.States.GameActive);
        }
        private void OnGameEnd()
        {
            SetGameState(GameEventPayLoad.States.GameEnded);
        }
        private void OnGamePost()
        {
            SetGameState(GameEventPayLoad.States.GamePost);
        }
        private void OnGameComplete()
        {
            SetGameState(GameEventPayLoad.States.Lobby);
        }
        private void OnLocalPlayerReady()
        {
            // This will be called fo every local client when ready is checked. Game Controller will verify if conditions are correct.
            if (gameController.LocalPlayer.isServer)
            {
                gameController.CmdStartMainCountdown();
            }
        }
        #endregion

        // Private functionality
        private IEnumerator InitSequence()
        {
            SetGameState(GameEventPayLoad.States.Initialize);

            yield return new WaitForSecondsRealtime(1f);

            if (NetworkController.s_InstanceExists)
            {
                // save ref
                networkController = NetworkController.s_Instance;
                // Add Event Call Backs
                networkController.LocalPlayerReadyEvent += OnLocalPlayerReady;
                networkController.NetworkManager.clientConnected += OnClientConnectedAction;
                networkController.NetworkManager.clientDisconnected += OnClientDisconnectedAction;
            } else
            {
                Debug.LogWarning("The Network manager instance cannot be found as it is Null.");
            }

            if (gameController != null)
            {
                // Add EventCallBacks
                gameController.GameInitializedEvent += OnGameInitialized;
                gameController.GameBeginEvent += OnGameBegin;
                gameController.GameEndEvent += OnGameEnd;
                gameController.GamePostEvent += OnGamePost;
                gameController.GameCompleteEvent += OnGameComplete;
            } else
            {
                Debug.LogWarning("The Game manager instance cannot be found as it is Null.");
            }

            if (DanceTogetherAudioManager.s_InstanceExists)
            {
                audioController = DanceTogetherAudioManager.s_Instance;
            }
            else 
            {
                Debug.LogWarning("The Audio manager instance cannot be found as it is Null.");
            }

            yield return new WaitForSeconds(0.5f);
            GoToMainMenu();
            yield return null;
        }

        public void Reset()
        {
            networkController?.Reset();
            gameController?.Reset();
            audioController?.Reset();
            SetGameState(GameEventPayLoad.States.MainMenu);
            Debug.Log("Global Reset Called");
        }

        protected override void OnDestroy()
        {
            // Remove Event Call Backs
            if (networkController != null)
            {
                networkController.LocalPlayerReadyEvent -= OnLocalPlayerReady;
                networkController.NetworkManager.clientConnected -= OnClientConnectedAction;
                networkController.NetworkManager.clientDisconnected -= OnClientDisconnectedAction;
            }

            if (gameController != null)
            {
                gameController.GameInitializedEvent -= OnGameInitialized;
                gameController.GameBeginEvent -= OnGameBegin;
                gameController.GameEndEvent -= OnGameEnd;
                gameController.GamePostEvent -= OnGamePost;
                gameController.GameCompleteEvent -= OnGameComplete;
            }

            base.OnDestroy();
        }
    }
}
