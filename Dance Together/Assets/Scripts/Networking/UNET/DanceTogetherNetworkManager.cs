using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using App.Controllers;

namespace App.Networking
{
    public class DanceTogetherNetworkManager : NetworkManager
    {
        /// <summary>
		/// Called on all clients when a player joins
		/// </summary>
		public event Action<DanceTogetherPlayer> playerJoined;
        /// <summary>
        /// Called on all clients when a player leaves
        /// </summary>
        public event Action<DanceTogetherPlayer> playerLeft;

        /// <summary>
        /// Called on a host when their server starts
        /// </summary>
        public event Action hostStarted;
        /// <summary>
        /// Called when the server is shut down
        /// </summary>
        public event Action serverStopped;
        /// <summary>
        /// Called when the client is shut down
        /// </summary>
        public event Action clientStopped;
        /// <summary>
        /// Called on a client when they connect to a game
        /// </summary>
        public event Action<NetworkConnection> clientConnected;
        /// <summary>
        /// Called on a client when they disconnect from a game
        /// </summary>
        public event Action<NetworkConnection> clientDisconnected;
        /// <summary>
        /// Called on a client when there is a networking error
        /// </summary>
        public event Action<NetworkConnection, int> clientError;
        /// <summary>
        /// Called on the server when there is a networking error
        /// </summary>
        public event Action<NetworkConnection, int> serverError;
        /// <summary>
        /// Called on clients and server when the scene changes
        /// </summary>
        //public event Action<bool, string> sceneChanged;
        /// <summary>
        /// Called on the server when all players are ready
        /// </summary>
        public event Action serverPlayersReadied;
        /// <summary>
        /// Called on the server when a client disconnects
        /// </summary>
        public event Action serverClientDisconnected;
 

        // private internal VARs
        private NetworkController controller;
        //  Init Var
        private bool isInitialized = false;
        public bool IsInitialized
        {
            get { return isInitialized; }
        }

        // public Vars
        public DanceTogetherPlayer danceTogetherPrefab;

        public List<PlayerController> PlayerList
        {
            get { return ClientScene.localPlayers; }
        }

        /// <summary>
        /// As an alternative to the singleton pattern. This "Init" pattern takes a bit more effort to maintain, but offers more flexibility in scene.
        /// Note : although that flexibility is not needed in the game currently
        /// </summary>
        public void Init(NetworkController _controller)
        {
            if(isInitialized)
            {
                Debug.LogError("The DanceTogether NetworkManager has already been Initialized. Canceling Attempt.");
                return;
            }
            isInitialized = true;
            controller = _controller;


            if (controller == null)
            {
                Debug.LogWarning("Network Controller for NetworkManager was assigned null. Please assign NetworkManager to NetworkController.");
            }
        }

        public void JoinGame(LanConnectionInfo _connectionInfo)
        {
            networkAddress = _connectionInfo.IpAddress;
            networkPort = _connectionInfo.Port;

            StartClient();

            if (MainController.s_Instance.verboseLogging) Debug.Log("Attempting to Join Lan-game at Adress : " + _connectionInfo.IpAddress + " : Using Port : " + _connectionInfo.Port);
        }

        
        public void RegisterClientPrefab(GameObject _object)
        {
            ClientScene.RegisterPrefab(_object);
            if (MainController.s_Instance.verboseLogging) Debug.Log("A prefab was registered with client : " + _object.name);
        }

        public void SpawnPrefab(GameObject _object)
        {
            NetworkServer.Spawn(_object);
            if (MainController.s_Instance.verboseLogging) Debug.Log("A prefab has been spawned on the server : " + _object.name);
        }

        public void Reset()
        {
            StopClient();
            StopHost();
            //Shutdown();
        }

        /// <summary>
		/// Gets the NetworkPlayer object for a given connection
		/// </summary>
		public static DanceTogetherPlayer GetPlayerForConnection(NetworkConnection conn)
        {
            return conn.playerControllers[0].gameObject.GetComponent<DanceTogetherPlayer>();
        }

        #region NetworkManager CallBacks
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            if(clientConnected != null)
            {
                clientConnected(conn);
            }

            if (MainController.s_Instance.verboseLogging) Debug.Log("Client Connected : " + conn.hostId.ToString());
        }
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            //base.OnClientDisconnect(conn);

            if(clientDisconnected != null)
            {
                clientDisconnected(conn);
            }
            StopClient();
            
            if (conn.lastError != NetworkError.Ok)
            {
                if (MainController.s_Instance.verboseLogging) { Debug.LogError("ClientDisconnected due to error: " + conn.lastError); }
            }
            if (MainController.s_Instance.verboseLogging) Debug.Log("Client disconnected from server: " + conn);
        }
        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            base.OnClientError(conn, errorCode);
            if (clientError != null)
            {
                clientError(conn, errorCode);
            }
            if (MainController.s_Instance.verboseLogging) Debug.Log("Client Error : " + conn.hostId.ToString() + " : code - " + errorCode.ToString());
        }
        public override void OnClientNotReady(NetworkConnection conn)
        {
            base.OnClientNotReady(conn);
            if (MainController.s_Instance.verboseLogging) Debug.Log("Client Not Ready : " + conn.hostId.ToString());
        }
        public override void OnDropConnection(bool success, string extendedInfo)
        {
            base.OnDropConnection(success, extendedInfo);
            if (MainController.s_Instance.verboseLogging) Debug.Log("Drop Connection : " + success + " : " + extendedInfo.ToString());
        }
        public override void OnServerConnect(NetworkConnection conn)
        {
            if (MainController.s_Instance.verboseLogging) Debug.LogFormat("OnServerConnect\nID {0}\nAddress {1}\nHostID {2}", conn.connectionId, conn.address, conn.hostId);

            if (numPlayers >= maxConnections ||
                controller.CurrentState != NetworkController.NetworkState.InLobby)
            {
                conn.Disconnect();
            }
            else
            {
                // Reset ready flags for everyone because the game state changed
                if (controller.CurrentState == NetworkController.NetworkState.InLobby)
                {
                    controller.ClearAllReadyStates();
                }
            }

            base.OnServerConnect(conn);
        }
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Debug.Log("OnServerDisconnect");
            base.OnServerDisconnect(conn);

            // Reset ready flags for everyone because the game state changed
            if (controller.CurrentState == NetworkController.NetworkState.InLobby)
            {
                controller.ClearAllReadyStates();
            }

            if (serverClientDisconnected != null)
            {
                serverClientDisconnected();
            }
        }
        public override void OnServerError(NetworkConnection conn, int errorCode)
        {
            base.OnServerError(conn, errorCode);
            if(serverError != null)
            {
                serverError(conn, errorCode);
            }
            if (MainController.s_Instance.verboseLogging) Debug.LogError("Server error : " + conn.hostId.ToString() + " : code - " + errorCode.ToString());
        }
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            //base.OnServerAddPlayer(conn, playerControllerId); // default

            /// Here We Replace the default prefab with our DanceTogetherPrefab
            DanceTogetherPlayer player = Instantiate<DanceTogetherPlayer>(danceTogetherPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player.gameObject, playerControllerId);

            //player.Init(controller);
            //controller.RegisterNewPlayer(player);

            if (playerJoined != null)
            {
                playerJoined(player);
            }

            if (MainController.s_Instance.verboseLogging) Debug.Log("Server Added player : " + conn.hostId.ToString() + /*" : playerprefab - " + player.name +*/ " : ID - " + playerControllerId);
        }
        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            base.OnServerRemovePlayer(conn, player);

            DanceTogetherPlayer dtPlayer = GetPlayerForConnection(conn);

            //controller.UnRegisterPlayer(dtPlayer);

            if(playerLeft != null)
            {
                playerLeft(dtPlayer);
            }

            if (MainController.s_Instance.verboseLogging) Debug.Log("Server Removed player : " + conn.hostId.ToString() + " : player - " + player.gameObject.name);
        }
        public override void OnServerReady(NetworkConnection conn)
        {
            base.OnServerReady(conn);
            if (MainController.s_Instance.verboseLogging) Debug.Log("Server is Ready : " + conn.hostId.ToString());
        }
        public override void OnStartHost()
        {
            base.OnStartHost();

            if (hostStarted != null)
            {
                hostStarted();
            }

            if (MainController.s_Instance.verboseLogging) Debug.Log("Host has Started");
        }
        public override void OnStopHost()
        {
            base.OnStopHost();
            if (MainController.s_Instance.verboseLogging) Debug.Log("Host Has Stopped");
        }
        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);
            if (MainController.s_Instance.verboseLogging) Debug.Log("client started on server : " + networkAddress);
        }
        public override void OnStopClient()
        {
            base.OnStopClient();
            if(clientStopped != null)
            {
                clientStopped();
            }
            if (MainController.s_Instance.verboseLogging) Debug.Log("client ended");
        }
        public override void OnStopServer()
        {
            base.OnStopServer();
            if (serverStopped != null)
            {
                serverStopped();
            }
            if(MainController.s_Instance.verboseLogging) Debug.Log("Server Has Stopped");
        }
        #endregion
    }
}