using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using FXSceneEditor;

public class FXMirrorPlayerLobbyInstance : NetworkBehaviour
{
    public static FXMirrorPlayerLobbyInstance localPlayer;
    
    [SerializeField ]private float refreshPortTimer = 5f; 
    private float timeRemaining;
    private bool isRunning = false;

    private FXMirrorServerPortManager serverPortManager;
    private List<PortEntry> cachedOpenPorts; // Cache for open ports
    private int cachedDesktopPort;
    private int cachedWebPort;
    private PortRoomSelectionView roomSelectionView;

    public event Action<List<PortEntry>> OnOpenPortsUpdate;

    #region SERVER
    [Command]
    public void CmdOpenPort(string playerName)
    {
        serverPortManager = FXMirrorServerPortManager.Instance;
        if (serverPortManager == null) return;

        Debug.Log("COMMAND OPEN PORT");
        PortEntry? portEntry = serverPortManager.OpenPort(playerName);
        List<PortEntry> openPorts = serverPortManager.GetOpenPorts();

        if (portEntry != null)
        {
            TargetReceiveOpenPort(connectionToClient, portEntry.Value, openPorts);
            BoardcastUpdateOpenPorts(openPorts); 
        }
    }

    [Command]
    public void CmdClosePort(string playerName)
    {
        serverPortManager = FXMirrorServerPortManager.Instance;
        if (serverPortManager == null) return;

        Debug.Log("COMMAND CLOSE PORT");
        PortEntry? portEntry = serverPortManager.ClosePort(playerName);
        List<PortEntry> openPorts = serverPortManager.GetOpenPorts();

        if (portEntry != null)
        {
            TargetReceiveClosePorts(connectionToClient, portEntry.Value, openPorts);
            BoardcastUpdateOpenPorts(openPorts);
        }
    }

    /// Not Used yet
    [Command]
    public void CmdOpenPort(int port)
    {
        serverPortManager = FXMirrorServerPortManager.Instance;
        if (serverPortManager == null) return;

        bool success = serverPortManager.OpenPort(port);
        TargetConfirmPortOpen(connectionToClient, success);
    }

    [Command]
    public void CmdCheckPortStatus(int port)
    {
        serverPortManager = FXMirrorServerPortManager.Instance;
        if (serverPortManager == null) return;

        Debug.Log($"Port {port} Open: {serverPortManager.IsPortOpen(port)}");
        bool isOpen = serverPortManager.IsPortOpen(port);
        TargetReceivePortStatus(connectionToClient, port, isOpen);
    }

    [Command]
    public void CmdGetOpenPorts()
    {
        serverPortManager = FXMirrorServerPortManager.Instance;
        if (serverPortManager == null)
        {
            Debug.Log("No server port manager");
            return;
        }

        List<PortEntry> openPorts = serverPortManager.GetOpenPorts();
        Debug.Log("CMD Open Ports: " + string.Join(", ", openPorts.Select(e => $"{e.playerName} ({e.desktopPort}, {e.webPort})")));
        TargetReceiveOpenPorts(connectionToClient, openPorts);
    }
    #endregion

    #region CLIENT
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (isLocalPlayer)
        {
            localPlayer = this;
            serverPortManager = FXMirrorServerPortManager.Instance;

            roomSelectionView = FindAnyObjectByType<PortRoomSelectionView>();
            if (roomSelectionView != null)
            {
                roomSelectionView.Setup(this);

                CmdGetOpenPorts();
            }
        }
    }

    public void RefreshGetOpenPortsList()
    {
        CmdGetOpenPorts();
    }

    public void OpenNewPort()
    {
        CmdOpenPort(PlayerDataHandler.Instance.playerName);
    }

    public void Test_CheckPortStatus(int port)
    {
        CmdCheckPortStatus(port);
    }

    [ClientRpc]
    private void BoardcastUpdateOpenPorts(List<PortEntry> openPorts)
    {
        cachedOpenPorts = openPorts; // Cache the result
        Debug.Log("List of open ports: " + string.Join(", ", cachedOpenPorts.Select(e => $"{e.playerName} ({e.desktopPort}, {e.webPort})")));

        OnOpenPortsUpdate?.Invoke(cachedOpenPorts);
    }

    [TargetRpc]
    private void TargetReceiveOpenPort(NetworkConnection target, PortEntry entry, List<PortEntry> openPorts)
    {
        cachedDesktopPort = entry.desktopPort;
        cachedWebPort = entry.webPort;
        cachedOpenPorts = openPorts; // Cache the result

        StartupSceneHandler.PortNumber = cachedDesktopPort;
        StartupSceneHandler.PortWeb = cachedWebPort;

        Debug.Log($"Newly Opened Ports - Desktop: {cachedDesktopPort}, Web: {cachedWebPort}");
        OnOpenPortsUpdate?.Invoke(cachedOpenPorts);
    }

    [TargetRpc]
    private void TargetReceiveClosePorts(NetworkConnection target, PortEntry entry, List<PortEntry> openPorts)
    {
        cachedDesktopPort = -1;
        cachedWebPort = -1;
        cachedOpenPorts = openPorts; // Cache the result

        Debug.Log($"Port {entry.playerName} is {(entry.isOpen ? "OPEN" : "CLOSED")}");
        OnOpenPortsUpdate?.Invoke(cachedOpenPorts);
    }

    [TargetRpc]
    private void TargetConfirmPortOpen(NetworkConnection target, bool success)
    {
        // Confirm port open status
    }

    [TargetRpc]
    private void TargetReceivePortStatus(NetworkConnection target, int port, bool isOpen)
    {
        Debug.Log($"Port {port} is {(isOpen ? "OPEN" : "CLOSED")}");
    }

    [TargetRpc]
    private void TargetReceiveOpenPorts(NetworkConnection target, List<PortEntry> openPorts)
    {
        cachedOpenPorts = openPorts; // Cache the result
        Debug.Log("List of open ports: " + string.Join(", ", cachedOpenPorts.Select(e => $"{e.playerName} ({e.desktopPort}, {e.webPort})")));

        OnOpenPortsUpdate?.Invoke(cachedOpenPorts);

        StartTimer();
    }
    #endregion

    #region Monobehaviour

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                RefreshGetOpenPortsList();
            }

            if (isRunning && timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    isRunning = false;
                    RefreshGetOpenPortsList();
                }
            }
        }
    }

    private void StartTimer()
    {
        timeRemaining = refreshPortTimer;
        isRunning = true;
    }

    private void StopTimer()
    {
        isRunning = false;
    }



    #endregion
}
