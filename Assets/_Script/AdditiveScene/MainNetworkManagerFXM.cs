using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;

namespace FXSceneEditor
{
    public class MainNetworkManagerFXM : NetworkManager
    {
        public static MainNetworkManagerFXM Instance { get; private set; }

        [Header("")]
        [SerializeField] kcp2k.KcpTransport _kcpTransport;
        [SerializeField] SimpleWebTransport _simpleWebTransport;

        [SerializeField] private bool _wasConnected;
        [SerializeField] private int _reconnectTry = 0;
        [SerializeField] private const int _maxReconnectTries = 20;

        private Dictionary<int, float> activeRooms = new Dictionary<int, float>(); // Room Port -> Last Heartbeat
        private UdpClient udpListener;
        private int listenPort; // UDP port for receiving heartbeats
        private CancellationTokenSource cancellationTokenSource;

        public override void Awake()
        {
            sendRate = 30;
            _kcpTransport.port = (ushort)StartupSceneHandler.PortNumber;
            _simpleWebTransport.port = (ushort)StartupSceneHandler.PortWeb;
            networkAddress = StartupSceneHandler.IpNetwork;

            base.Awake();
            if (Instance != null)
            {
                Destroy(this);
            }
            Instance = this;
        }

        public override void Start()
        {
            if (StartupSceneHandler.IsServer)
            {
                StartServer();

                StartListeningForHeartbeats();
                InvokeRepeating(nameof(CheckForInactiveRooms), 10f, 10f);
            }
            else
            {
                StartClient();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (!StartupSceneHandler.IsServer)
            {
                Debug.Log("Shutdown Client");
                NetworkClient.Shutdown();
            }
        }

        #region SERVER
        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("OnStartServer : " + networkAddress + ", port : " + _kcpTransport.port);
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("OnServerConnect " + NetworkManager.singleton.numPlayers);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log("OnServerDisconnect " + NetworkManager.singleton.numPlayers);
        }

        [Server]
        private void StartListeningForHeartbeats()
        {
            listenPort = StartupSceneHandler.PortUdpServerBridge;

            udpListener = new UdpClient(listenPort);
            udpListener.BeginReceive(OnReceiveHeartbeat, null);
            Debug.Log("Lobby listening for room heartbeats...");
        }

        [Server]
        private void OnReceiveHeartbeat(IAsyncResult result)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
            byte[] data = udpListener.EndReceive(result, ref endPoint);
            string message = Encoding.UTF8.GetString(data);

            string[] parts = message.Split(':');
            if (parts.Length < 2) return;

            int roomPort = int.Parse(parts[0]);
            string status = parts[1];

            if (status == "ACTIVE")
            {
                activeRooms[roomPort] = Time.time;

                Debug.Log($"Heartbeat from {roomPort}: Active");
            }
            else if (status == "CLOSED")
            {
                if (activeRooms.ContainsKey(roomPort))
                {
                    activeRooms.Remove(roomPort);

                    // Close room port
                    FXMirrorServerPortManager.Instance.ClosePort(roomPort);
                    

                    Debug.Log($"Room {roomPort} closed.");
                }
            }

            // Keep listening
            udpListener.BeginReceive(OnReceiveHeartbeat, null);
        }

        [Server]
        private void CheckForInactiveRooms()
        {
            float currentTime = Time.time;
            List<int> toRemove = new List<int>();

            foreach (var room in activeRooms)
            {
                if (currentTime - room.Value > 15f) // If no heartbeat for 15 seconds
                {
                    Debug.Log($"Room {room.Key} is inactive. Removing.");
                    toRemove.Add(room.Key);
                }
            }

            foreach (int port in toRemove)
            {
                // Close room port
                FXMirrorServerPortManager.Instance.ClosePort(port);

                activeRooms.Remove(port);
            }
        }
        #endregion SERVER

        #region CLIENT
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("OnClientConnected");
            //LoadingController.Instance.ShowHideLoadingScreen(false);
            //LoadingController.Instance.RunFakeLoading();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("OnClientDisconnected");
            if (!_wasConnected)
            {
                if (_reconnectTry > _maxReconnectTries) return;
                _reconnectTry++;

                print("trying reconnect " + _reconnectTry);
                Invoke(nameof(StartClient), 2f);
            }
        }
        #endregion CLIENT

        #region Monobehaviour

        #endregion
    }
}