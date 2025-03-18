using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using FXSceneEditor.Network;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FXSceneEditor
{
    public class AdditiveNetworkManagerFXM : NetworkManager
    {
        public static new AdditiveNetworkManagerFXM Instance { get; private set; }

        public static Dictionary<string, string> NetworkedSceneDatas = new Dictionary<string, string>();
        public static Action<string> LoadScene;
        public static Action<string> UnloadScene;

        [Space(5f)]
        [SerializeField] kcp2k.KcpTransport _kcpTransport;
        [SerializeField] SimpleWebTransport _simpleWebTransport;
        [SerializeField] List<NetworkedSceneData> _networkedSceneDatas;

        [Header("SERVER FLAGS")]
        [SerializeField] bool _isSubScenesLoaded;

        [Header("CLIENT FLAGS")]
        [SerializeField] bool _isInTransition;

        [Space(5f)]
        [SerializeField] List<string> _subsceneNames = new List<string>();

        [SerializeField] private bool _wasConnected;
        [SerializeField] private int _reconnectTry = 0;
        [SerializeField] private const int _maxReconnectTries = 20;
        [SerializeField] private string _guestName = "";
        [SerializeField] private string _scenarioKey = "";
        [SerializeField] private bool _isFirstPlayerConnected;

        private float lastPlayerTime;
        private bool shuttingDown = false;
        private UdpClient udpClient;
        private string lobbyIP; // Lobby Server IP
        private int lobbyPort; // UDP port for heartbeats
        private int roomPort; // The port this room server is running on

        private async void OnUnloadScene(string sceneName)
        {
            await UniTask.Delay(1000);
            _isInTransition = false;
            Debug.Log("OnLoadScene : " + sceneName);
        }

        private void OnLoadScene(string sceneName)
        {
            _isInTransition = false;
            Debug.Log("OnLoadScene : " + sceneName);
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            sendRate = 30;
            _kcpTransport.port = (ushort)StartupSceneHandler.PortNumber;
            _simpleWebTransport.port = (ushort)StartupSceneHandler.PortWeb;
            networkAddress = StartupSceneHandler.IpNetwork;

            base.Awake();
            Instance = this;
        }

        public async override void Start()
        {
            if (_subsceneNames.Count != 0)
            {
                _subsceneNames.Clear();
            }

            _subsceneNames = StartupSceneHandler.SceneNames;

            if (NetworkedSceneDatas.Count != 0)
            {
                NetworkedSceneDatas.Clear();
            }

            foreach (var _networkedSceneData in _networkedSceneDatas)
            {
                NetworkedSceneDatas.Add(_networkedSceneData.Key, _networkedSceneData.SceneName);
            }

            if (StartupSceneHandler.IsServer)
            {
                lastPlayerTime = Time.time;
                MultiplexTransport multiplex = transport.GetComponent<MultiplexTransport>();

                if (multiplex != null)
                {
                    foreach (var t in multiplex.transports)
                    {
                        if (t is KcpTransport kcp)
                        {
                            roomPort = kcp.Port; // Get port from KCP
                            Debug.Log($"[Room Server] Using KCP Transport, Port: {roomPort}");
                            break;
                        }
                        else if (t is SimpleWebTransport web)
                        {
                            roomPort = web.port; // Get port from SimpleWeb
                            Debug.Log($"[Room Server] Using SimpleWeb Transport, Port: {roomPort}");
                            break;
                        }
                    }
                }

                Debug.Log("Setup UDP");
                udpClient = new UdpClient();
                Debug.Log("UDP null: " + udpClient == null);

                lobbyIP = StartupSceneHandler.IpNetwork;
                lobbyPort = StartupSceneHandler.PortUdpServerBridge;

                SendHeartbeat(); // Notify the lobby that this room is active
                InvokeRepeating(nameof(SendHeartbeat), 5f, 5f);

                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} Trigger Start Server");
                _guestName = StartupSceneHandler.GuestName;
                _scenarioKey = StartupSceneHandler.ScenarioKey;
                StartServer();
            }
            else
            {
                await UniTask.Delay(5000);
                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} Trigger Start Client");
                StartClient();
            }
        }

        void OnEnable()
        {
            LoadScene += OnLoadScene;
            UnloadScene += OnUnloadScene;
        }

        void OnDisable()
        {
            LoadScene -= OnLoadScene;
            UnloadScene -= OnUnloadScene;
        }

        public override void Update()
        {
            base.Update();

            if (!NetworkServer.active) return; // Only check for shutdown on the Room Server

            if (numPlayers > 0) return;

            if (Time.time - lastPlayerTime > 60f && !shuttingDown)
            {
                Debug.Log("[Room Server] No players for 1 minute. Shutting down...");
                shuttingDown = true;
                SendShutdownMessage();
                ShutdownServer();
            }
        }

        #region Scene Management
        public override void OnStartServer()
        {
            base.OnStartServer();

            //PostScenarioServerData();
            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnStartServer {_kcpTransport.port} {_simpleWebTransport.port} {StartupSceneHandler.IsMainProcess} {StartupSceneHandler.IsServer} {StartupSceneHandler.PortNumber} {StartupSceneHandler.PortWeb} {StartupSceneHandler.StartSceneName} ");
            StartCoroutine(ServerLoadSubScenes());
        }

        //void PostScenarioServerData(){
        //    DatabaseController.IsGuest = true;
        //    DatabaseController.SaveData(new Dictionary<string, int> {
        //        {_scenarioKey+".port_number", StartupSceneHandler.PortNumber},
        //        {_scenarioKey+".port_web", StartupSceneHandler.PortWeb},
        //    }, _guestName, true);

        //    DatabaseController.SaveData(new Dictionary<string, bool> {
        //        {_scenarioKey+".is_active", true},
        //    }, _guestName, true);
        //}

        public override void OnStopServer()
        {
            _isSubScenesLoaded = false;
            _isInTransition = false;
            _isFirstPlayerConnected = false;
        }

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        /// </summary>
        /// <param name="sceneName">The name of the new scene.</param>
        public override void OnServerSceneChanged(string sceneName)
        {
            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnServerSceneChanged {sceneName} ");
            // This fires after server fully changes scenes, e.g. offline to online
            // If server has just loaded the Container (online) scene, load the subscenes on server
            // if (sceneName == onlineScene){
            //     Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnServerSceneChanged 2 {sceneName} ");
            //     StartCoroutine(ServerLoadSubScenes());
            // }
        }

        IEnumerator ServerLoadSubScenes()
        {
            for (int i = 0; i < _subsceneNames.Count; i++)
            {
                yield return SceneManager.LoadSceneAsync(_subsceneNames[i], new LoadSceneParameters
                {
                    loadSceneMode = LoadSceneMode.Additive,
                    localPhysicsMode = LocalPhysicsMode.Physics3D // change this to .Physics2D for a 2D game
                });
                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} ServerLoadSubScenes  {_subsceneNames[i]} ");
            }

            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} ServerLoadSubScenes  All Scene Loaded ");
            _isSubScenesLoaded = true;
        }

        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="sceneName">Name of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
        public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
        {
            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientChangeScene {sceneName} {sceneOperation}");

            if (sceneOperation == SceneOperation.UnloadAdditive)
            {
                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientChangeScene Unload {sceneName} {sceneOperation}");
                UnloadAdditive(sceneName);
            }

            if (sceneOperation == SceneOperation.LoadAdditive)
            {
                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientChangeScene Load {sceneName} {sceneOperation}");
                LoadAdditive(sceneName);
            }
        }

        // IEnumerator LoadAdditive(string sceneName)
        async void LoadAdditive(string sceneName)
        {
            while (_isInTransition)
            {
                await UniTask.Yield();
            }

            _isInTransition = true;

            // host client is on server...don't load the additive scene again
            if (mode == NetworkManagerMode.ClientOnly)
            {
                // Start loading the additive subscene
                loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                // LoadingController.Instance.ShowLoading(loadingSceneAsync);
                //LoadingController.Instance.Load(sceneName, true);

                while (!loadingSceneAsync.isDone)
                {
                    await UniTask.Yield();
                }
                //while (_isInTransition)
                //{
                //    await UniTask.Yield();
                //}

                await UniTask.WaitForEndOfFrame(this);

                SceneManager.SetActiveScene(SceneManager.GetSceneByPath(sceneName));
            }

            // Reset these to false when ready to proceed
            NetworkClient.isLoadingScene = false;
            _isInTransition = false;

            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} LoadAdditive  {sceneName} ");
            OnClientSceneChanged();
        }

        async void UnloadAdditive(string sceneName)
        {
            _isInTransition = true;
            //LoadingController.Instance.ShowHideLoadingScreen(true);

            // host client is on server...don't unload the additive scene here.
            if (mode == NetworkManagerMode.ClientOnly)
            {
                UnloadCurrentScene(sceneName);

                while (_isInTransition)
                {
                    await UniTask.Yield();
                }
            }

            // Reset these to false when ready to proceed
            NetworkClient.isLoadingScene = false;
            _isInTransition = false;

            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} UnloadAdditive  {sceneName} ");
            OnClientSceneChanged();
        }

        async void UnloadCurrentScene(string sceneName)
        {
            await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByPath(sceneName));
            await Resources.UnloadUnusedAssets();
        }
        /// <summary>
        /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
        /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        /// <param name="conn">The network connection that the scene change message arrived on.</param>
        public override void OnClientSceneChanged()
        {
            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientSceneChanged");
            // Only call the base method if not in a transition.
            // This will be called from DoTransition after setting doingTransition to false
            // but will also be called first by Mirror when the scene loading finishes.
            if (!_isInTransition)
            {
                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientSceneChanged 2");
                base.OnClientSceneChanged();
            }
        }

        public override void OnClientDisconnect()
        {
            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientDisconnect");
            if (!_wasConnected)
            {
                if (_reconnectTry > _maxReconnectTries) return;
                _reconnectTry++;

                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} trying reconnect {_reconnectTry}");
                Invoke(nameof(StartClient), 4f);
            }
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);

            if (!_isFirstPlayerConnected)
            {
                _isFirstPlayerConnected = true;
                Invoke(nameof(CheckLastPlayerConnected), 180);
            }

            lastPlayerTime = Time.time; // Reset timer when a player joins
            Debug.Log($"Player joined. Resetting inactivity timer.");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            CheckForShutdown();
        }

        private void CheckForShutdown()
        {
            if (numPlayers == 0)
            {
                lastPlayerTime = Time.time; // Start countdown for shutdown
            }
        }

        [Server]
        private void SendHeartbeat()
        {
            if (!NetworkServer.active)
            {
                Debug.Log("Not server");
                return; // Only send heartbeats if this is the Room Server
            }

            if (shuttingDown) return;

            if (udpClient == null)
            {
                Debug.LogError("[Room Server] udpClient is NULL! Re-initializing...");
                udpClient = new UdpClient();
            }

            string message = $"{roomPort}:ACTIVE";
            byte[] data = Encoding.UTF8.GetBytes(message);

            try
            {
                udpClient.Send(data, data.Length, lobbyIP, lobbyPort);
                Debug.Log($"[Room Server] Sent heartbeat to Lobby ({lobbyIP}:{lobbyPort}): {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Room Server] Failed to send heartbeat: {e.Message}");
            }
        }

        [Server]
        private void SendShutdownMessage()
        {
            if (!NetworkServer.active) return; // Only run this on the Room Server

            if (udpClient == null)
            {
                Debug.LogError("[Room Server] udpClient is NULL! Re-initializing...");
                udpClient = new UdpClient();
            }

            string message = $"{roomPort}:CLOSED";
            byte[] data = Encoding.UTF8.GetBytes(message);

            try
            {
                udpClient.Send(data, data.Length, lobbyIP, lobbyPort);
                Debug.Log($"[Room Server] Sent shutdown message to Lobby ({lobbyIP}:{lobbyPort}): {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Room Server] Failed to send shutdown message: {e.Message}");
            }
        }

        [Server]
        private void ShutdownServer()
        {
            Debug.Log("Closing Room Server...");
            Application.Quit();
        }

        void CheckLastPlayerConnected()
        {
            Debug.Log("Client Count : " + NetworkManager.singleton.numPlayers);

            if (NetworkManager.singleton.numPlayers > 0)
            {
                Invoke(nameof(CheckLastPlayerConnected), 60);
            }
            else
            {
                //Invoke(nameof(DeleteScenarioServerData), 10);
            }
        }

        //void DeleteScenarioServerData(){
        //    if (NetworkManager.singleton.numPlayers > 0){
        //        Invoke(nameof(CheckLastPlayerConnected), 60);
        //    }else{
        //        DatabaseController.IsGuest = true;
        //        DatabaseController.DeleteData(new List<string>() { _scenarioKey }, onSuccess => {
        //            if (onSuccess.IsStringValid()){
        //                var json = JsonConvert.DeserializeObject<RequestResponse<string>>(onSuccess);
        //                if (json.status){
        //                    Application.Quit();
        //                }
        //            }
        //        }, onError =>{

        //        }, _guestName, true);
        //    }
        //}
        #endregion

        #region Server System Callbacks
        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnServerReady {conn.identity}, {conn.connectionId}");

            // This fires from a Ready message client sends to server after loading the online scene
            base.OnServerReady(conn);

            if (conn.identity == null)
            {
                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnServerReady 2 {conn.identity}, {conn.connectionId}");
                StartCoroutine(AddPlayerDelayed(conn));
            }
        }

        // This delay is mostly for the host player that loads too fast for the
        // server to have subscenes async loaded from OnServerSceneChanged ahead of it.
        IEnumerator AddPlayerDelayed(NetworkConnectionToClient conn)
        {
            // Wait for server to async load all subscenes for game instances
            while (!_isSubScenesLoaded)
            {
                Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} _isSubScenesLoaded {conn.identity}, {conn.connectionId}");
                yield return null;
            }

            // Send Scene msg to client telling it to load the first additive scene
            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} Tell Client {NetworkedSceneDatas[_subsceneNames[0]]}");
            conn.Send(new SceneMessage { sceneName = NetworkedSceneDatas[_subsceneNames[0]], sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

            // We have Network Start Positions in first additive scene...pick one
            //Transform start = GetStartPosition();
            //var startPositions = GameObject.FindGameObjectsWithTag("StartPosition");
            //var startPosInTheSameScene = startPositions.Where(startPost => startPost.scene.name.Equals(NetworkedSceneDatas[_subsceneNames[0]]));
            //Transform start = startPosInTheSameScene.ElementAt(UnityEngine.Random.Range(0, startPosInTheSameScene.Count() - 1)).transform;

            // Instantiate player as child of start position - this will place it in the additive scene
            // This also lets player object "inherit" pos and rot from start position transform
            //GameObject player = Instantiate(playerPrefab, start);
            GameObject player = Instantiate(playerPrefab);
            player.name = $"{playerPrefab.name} [connId = {conn.identity}, {conn.connectionId} ]";
            player.transform.localScale = Vector3.one;
            // now set parent null to get it out from under the Start Position object
            //player.transform.SetParent(null);

            SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(NetworkedSceneDatas[_subsceneNames[0]]));

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            // Finally spawn the player object for this connection
            NetworkServer.AddPlayerForConnection(conn, player);
            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} AddPlayerForConnection {conn.identity}, {conn.connectionId}");
        }
        #endregion
    }
}