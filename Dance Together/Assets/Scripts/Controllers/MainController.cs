using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using App.Data;
using App.Events;
using App.Networking;
using App.Audio;

namespace App.Controllers
{
    public class MainController : MonoBehaviour
    {
        // private VARS
        // inspector visible Vars
        [Header("Game Settings")]
        [SerializeField]
        private GameType currentGameType; // this might make more sense in Game Controller.

        //[SerializeField]
        //private GameCommandList commands;

        [Header("Game Events")]
        // Events
        [SerializeField]
        private GameEvent gameStateEvent;

        // gameObject references.
        [Header("Controller Linkage")]
        [SerializeField]
        private NetworkController networkController;
        [SerializeField]
        private DanceTogetherGameManager gameController;
        [SerializeField]
        private DanceTogetherAudioManager audioController;

        [Header("Dev Options")]
        public bool verboseLogging = false; // use to get deep messaging in logs about networking.

        // hidden Vars
        [SerializeField]
        private GameEventPayLoad.States currentGameState;


        // Getter VARS
        public NetworkController NetworkController
        {
            get { return networkController; }
        }
        public DanceTogetherGameManager GameController
        {
            get { return gameController; }
        }
        public DanceTogetherAudioManager AudioController
        {
            get { return audioController; }
        }
        public GameType CurrentGameType
        {
            get { return currentGameType; }
        }

        // MonoBehaviour Functionality.
        /// <summary>
        /// Using the Monobehavior Method "Awake" to begin the "Init" Cascade sequence.
        /// The purpose of this has a few main Advantages. 
        /// Advantages :
        /// 1. To force a Primary flow of controller logic, to make changes and reading of code a bit more Straight forward.
        /// 2. With this method, multiple controllers of various types can be Controlled in a single scene.
        /// 3. The "MainController" can act as a State-Machine That ensures all other managers and sub controllers are acting as intended.
        /// Disadvantages:
        /// 1. Compared to a singleton strategy, This method has issues if loading scenes that may have the same redundant controllers placed for testing or intent.
        /// 2. Not as easy to grab reference to a singular controller from random scripts.
        /// 
        /// Notes :
        /// Currently in DanceTogether The Controllers are designed to always be in the "MainScene", or persistant scene frome Beggining to End.
        /// How to handle this Can be accomplished by a custom SceneManager, that will load in all additional levels with the Async load scene methods.
        /// It will be important for those Async loaded levels to not have competing controllers placed in them.
        /// Currently I have NOT implemented a scenemanager to do this, as the current game doesnt seem complex enough to support scene change over prefab loading.
        /// -Brandon 03/29/2020
        /// </summary>
        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            StopAllCoroutines();
            StartCoroutine(InitSequence());
        }

        // Public functionality
        public void AutoConnect()
        {
            networkController.AutoJoinGame(); // TODO - this function is currently empty.
            //networkManager.minPlayers = currentGameType.Data.MinPlayers;
            //networkManager.AutoConnect();
            //lobbyEnterEvent?.Raise();
            //commands.JoinedLobbyEvent?.Raise();
        }
        public void CreateGame()
        {
            // start game as host.
            networkController.StartNewLanGame();
            //SetGameState(GameEventPayLoad.States.Lobby);
        }

        public void JoinGame()
        {
            // browse games
            networkController.StartSearching();
            SetGameState(GameEventPayLoad.States.SearchingForGame);
        }

        public void GoToMainMenu()
        {
            // leave any game and return to main menu
            SetGameState(GameEventPayLoad.States.MainMenu);
            networkController.Reset();
            gameController.Reset();
            audioController.Reset();
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
            audioController.BeginLobbyMusic();
        }
        private void OnClientDisconnectedAction(NetworkConnection conn)
        {
            GoToMainMenu();
        }
        #endregion

        #region CallBacks 
        public void OnGameInitialized()
        {
            SetGameState(GameEventPayLoad.States.GameInitialize);
        }
        public void OnGameBegin()
        {
            SetGameState(GameEventPayLoad.States.GameActive);
        }
        public void OnGameEnd()
        {
            SetGameState(GameEventPayLoad.States.GameEnded);
        }
        public void OnGamePost()
        {
            SetGameState(GameEventPayLoad.States.GamePost);
        }
        public void OnGameComplete()
        {
            SetGameState(GameEventPayLoad.States.Lobby);
        }
        public void OnLocalPlayerReady()
        {
            // This will be called fo every local client when ready is checked. Game Controller will verify if conditions are correct.
            if (networkController.LocalPlayer.isServer)
            {
                gameController.CmdStartMainCountdown();
            }
        }
        #endregion

        // Private functionality
        private IEnumerator InitSequence()
        {
            SetGameState(GameEventPayLoad.States.Initialize);
            //commands.ClearAllCommandData();
            if (networkController != null)
            {
                networkController.Init(this);

                // Add Event Call Backs
                networkController.NetworkManager.clientConnected += OnClientConnectedAction;
                networkController.NetworkManager.clientDisconnected += OnClientDisconnectedAction;
            } else
            {
                Debug.LogWarning("The Network manager cannot be Initialized as it is Null.");
            }

            if (gameController != null)
            {
                gameController.Init(this);

                gameController.GameInitializedEvent += OnGameInitialized;
                gameController.GameBeginEvent += OnGameBegin;
                gameController.GameEndEvent += OnGameEnd;
                gameController.GamePostEvent += OnGamePost;
                gameController.GameCompleteEvent += OnGameComplete;



            } else
            {
                Debug.LogWarning("The Game manager cannot be Initialized as it is Null.");
            }

            if(audioController != null)
            {
                audioController.Init(this);
            }
            else
            {
                Debug.LogWarning("The Audio manager cannot be Initialized as it is Null.");
            }

            yield return new WaitForSeconds(0.5f);
            GoToMainMenu();
            yield return null;
        }

        public void Reset()
        {
            networkController.Reset();
            gameController.Reset();
            audioController.Reset();
            SetGameState(GameEventPayLoad.States.MainMenu);
            Debug.Log("Global Reset Called");
        }

        private void OnDestroy()
        {
            // Remove Event Call Backs
            if (networkController?.NetworkManager != null)
            {
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
        }
    }
}
