using System.Collections.Generic;
using UnityEngine;
using App.Controllers;
using System.Collections;
using UnityEngine.Networking;

namespace App.Networking
{
    public class NetworkController : MonoBehaviour
    {

        // networking states
        public enum NetworkState
        {
            Inactive,
            Pregame,
            Connecting,
            InLobby,
            InGame
        }

        public NetworkState CurrentState
        {
            get;
            private set;
        }

        [SerializeField]
        private DanceTogetherNetworkManager networkManager;
        [SerializeField]
        private DanceTogetherNetworkDiscovery networkDiscovery;
        [SerializeField]
        private GameListController gameListController;
        [SerializeField]
        private LobbyViewManager lobbyViewManager;

        [SerializeField]
        private List<DanceTogetherPlayer> playerList = new List<DanceTogetherPlayer>();
        public List<DanceTogetherPlayer> PlayerList
        {
            get { return playerList; }
        }

        [SerializeField]
        private DanceTogetherPlayer localPlayer = null;
        public DanceTogetherPlayer LocalPlayer
        {
            get { return localPlayer; }
            set { localPlayer = value; }
        }

        public bool CheckAllPlayersReady
        {
            get
            {
                foreach(DanceTogetherPlayer player in playerList)
                {
                    if (!player.IsReady)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
		/// Gets whether or not we're a server
		/// </summary>
		public static bool s_IsServer
        {
            get
            {
                return NetworkServer.active;
            }
        }

        private MainController controller;

        public MainController Controller
        {
            get { return controller; }
        }

        public bool verboseLogging
        {
            get { return controller.verboseLogging; }
        }

        public DanceTogetherNetworkManager NetworkManager
        {
            get { return networkManager; }
        }

        public DanceTogetherNetworkDiscovery NetworkDiscovery
        {
            get { return networkDiscovery; }
        }

        public GameListController GameListController
        {
            get { return gameListController; }
        }
        public LobbyViewManager LobbyViewManager
        {
            get { return lobbyViewManager; }
        }

        public void Init(MainController _controller)
        {
            controller = _controller;
            networkManager.Init(this);
            networkDiscovery.Init(this);
            gameListController?.Init(this);
            lobbyViewManager?.Init(this);
        }

        public void StartNewLanGame()
        {
            CurrentState = NetworkState.InLobby;
            networkManager.StartHost();
            networkDiscovery.StartServerBroadcast();
        }
        public void StopLanGame()
        {
            CurrentState = NetworkState.Inactive;
            if (networkManager.isNetworkActive)
            {
                networkManager.Reset();
                networkDiscovery.Reset();
            }
        }

        public void JoinLanGame()
        {
            CurrentState = NetworkState.InLobby;
            networkManager.StartClient();

            networkDiscovery.Reset();
        }

        public void JoinSpecificGame(LanConnectionInfo _connectionInfo)
        {
            CurrentState = NetworkState.InLobby;
            networkManager.JoinGame(_connectionInfo);

            networkDiscovery.Reset();
        }

        public void LeaveLanGame()
        {
            CurrentState = NetworkState.Inactive;
            if (networkManager.isNetworkActive)
            {
                networkManager.StopClient();
                //networkDiscovery.StartAsClient();
            }
        }

        public void StartSearching()
        {
            networkDiscovery.StartClientBroadcast();
        }

        public void AutoJoinGame()
        {
            StopAllCoroutines(); // Stop this crazy thing!
            StartCoroutine(AutoJoinSequence()); // Engage!
        }

        public void RegisterPrefab(GameObject _object)
        {
            networkManager.RegisterClientPrefab(_object);
        }

        public void SpawnNetworkGameObject(GameObject _object)
        {
            networkManager.SpawnPrefab(_object);
        }

        public void ClearAllReadyStates()
        {
            foreach (DanceTogetherPlayer player in playerList)
            {
                player.CmdClearReady();
            }
        }


        public void Reset()
        {

            networkManager.Reset();
            networkDiscovery.Reset();
        }

        #region Player Registration Methods
        // !!! These Methods are called from DanceTogetherPlayer script when they are created by the DanceTogetherNetworkManager !!!
        public void RegisterNewPlayer(DanceTogetherPlayer _player)
        {
            if(_player == null)
            {
                if (verboseLogging) Debug.LogWarning("A new Player Attempted to register, but was null.");
                return;
            }

            if(!playerList.Contains(_player))
            {
                playerList.Add(_player); // add to global 
                _player.PlayerID = playerList.IndexOf(_player) + 1; // set player id number // assigns color as well.

                lobbyViewManager?.AddNewLobbyIcon(_player);
                _player.playerReadyEvent += OnLocalPlayerReady;

                if (_player.isLocalPlayer)
                {
                    localPlayer = _player; // save local player
                }

                if (verboseLogging) Debug.Log("Registered DanceTogetherPlayer : ID " + _player.netId);
            }
            else if(verboseLogging)
            {
                Debug.LogWarning("A new network player attempted to register, but is already registered : " + _player.name + " : ID - " + _player.netId);
            }
        }

        public void UnRegisterPlayer(DanceTogetherPlayer _player)
        {
            if (_player == null)
            {
                if (verboseLogging) Debug.LogWarning("A Player Attempted to unregister, but was null.");
                return;
            }

            if (playerList.Contains(_player))
            {
                lobbyViewManager?.RemoveLobbyIcon(_player);
                //controller?.PreparePlayerCallbacks(_player, false);

                playerList.Remove(_player); // remove to global list
                _player.playerReadyEvent -= OnLocalPlayerReady;

                if (_player.isLocalPlayer)
                {
                    localPlayer = null; // remove local player
                }

                if (verboseLogging) Debug.Log("Unregistered DanceTogetherPlayer : ID " + _player.netId);
            }
            else if (verboseLogging)
            {
                Debug.LogWarning("A network player attempted to unregister, but is not currently registered : " + _player.name + " : ID - " + _player.netId);
            }
        }
        #endregion

        #region private CallBacks
        private void OnLocalPlayerReady(DanceTogetherPlayer _player)
        {
            controller?.OnLocalPlayerReady();
        }
        #endregion

        #region MonoBehaviour Methods
        private void OnEnable()
        {
            foreach(var player in playerList)
            {
                player.playerReadyEvent += OnLocalPlayerReady;
            }
        }
        private void OnDisable()
        {
            foreach (var player in playerList)
            {
                player.playerReadyEvent -= OnLocalPlayerReady;
            }
        }
        #endregion

        #region AutoJoinSequence
        private IEnumerator AutoJoinSequence()
        {
            // call auto join callback here -TODO
            bool attemptingJoin = true;
            bool joinSuccess = false;
            int joinAttempts = 3; // track number of join search attempts. : we will try 3 times.

            //Reset(); // reset - stop any current server activity.
            yield return new WaitForSecondsRealtime(0.5f); // wait for reset : 500 milliseconds

            networkDiscovery.StartClientBroadcast(); // start network discovery.

            while (attemptingJoin)
            {
                // Attempt to search for joinable games.itterate through number of attempts every 1 second
                for (int i = 0; i < joinAttempts; i++)
                {
                    yield return new WaitForSecondsRealtime(1f); // wait for 1 second.
                    Debug.Log("join attempts : " + i);
                    if (networkDiscovery.LanAdresses.Count > 0) // check if lanAdresses exist
                    {
                        networkManager.JoinGame(networkDiscovery.LanAdresses[0]);
                        joinSuccess = true; // join was success
                        attemptingJoin = false; // stop attempting to join
                    }
                }

                attemptingJoin = false; // failed to join all attempts, end while loop.
            }

            if (!joinSuccess)
            {
                // join failed call back?
                Debug.Log("Auto Join Failed to find a game in 3 seconds : Going to create game Meow.");
                controller.CreateGame(); // tell master controller to create a new game.
            }
            yield return null;
        }
        #endregion


    }
}