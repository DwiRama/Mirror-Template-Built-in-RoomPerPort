using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FXMirrorServerPortManager : NetworkBehaviour
{
    private static FXMirrorServerPortManager _instance;
    public static FXMirrorServerPortManager Instance => _instance;

    [Range(7000, 8999), SerializeField] private int initialStartPortPoint = 7000;
    [Range(7001, 9000), SerializeField] private int initialEndPortPoint = 7002;

    private int startPortPoint;
    private int endPortPoint;
    private List<PortEntry> portList = new List<PortEntry>();

    #region SERVER

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // If port point range doesn't make sense
        if (initialStartPortPoint > initialEndPortPoint)
        {
            startPortPoint = 8000;
            endPortPoint = 9000;
        }
        else
        {
            startPortPoint = initialStartPortPoint;
            endPortPoint = initialEndPortPoint;
        }
        Debug.Log($"Start Port Point: {startPortPoint}\n End Port Point: {endPortPoint}");
    }

    public bool IsPortOpen(int port) => portList.Exists(entry => (entry.desktopPort == port || entry.webPort == port) && entry.isOpen);

    public List<PortEntry> GetOpenPorts()
    {
        List<PortEntry> openEntries = new List<PortEntry>();
        foreach (var entry in portList)
        {
            if (entry.isOpen)
            {
                openEntries.Add(entry);
            }
        }

        Debug.Log("Open Ports: " + string.Join(", ", openEntries.Select(e => $"{e.playerName} ({e.desktopPort}, {e.webPort})")));
        return openEntries;
    }

    public PortEntry? OpenPort(string playerName)
    {
        // Check if player already has an open port
        if (portList.Exists(e => e.playerName == playerName))
        {
            Debug.LogWarning($"Player {playerName} already has an open port!");
            return null;
        }

        for (int i = startPortPoint; i < endPortPoint - 1; i += 2)
        {
            if (!IsPortOpen(i) && !IsPortOpen(i + 1))
            {
                Debug.Log("Opening port");

                var newEntry = new PortEntry(i, i + 1, playerName);
                newEntry.OpenPort(); // Ensure the port is actually opened

                portList.Add(newEntry);
                return newEntry;
            }
        }
        return null;
    }

    public PortEntry? ClosePort(string playerName)
    {
        PortEntry entry = portList.Find(e => e.playerName == playerName);
        if (entry.playerName != string.Empty)
        {
            Debug.Log($"Closing port for {playerName}");
            int index = portList.FindIndex(e => e.playerName == playerName);

            portList[index].ClosePort(); // Ensure the port is actually closed
            portList.RemoveAt(index);    // Remove from active ports

            Debug.Log($"Port {entry.desktopPort} & {entry.webPort} is closed");

            return entry;
        }
        Debug.LogWarning($"Player {playerName} has no open ports to close.");
        return null;
    }

    public PortEntry? ClosePort(int portNumber)
    {
        PortEntry entry = portList.Find(e => e.desktopPort == portNumber || e.webPort == portNumber);

        if (entry.playerName != string.Empty)
        {
            Debug.Log($"Closing port {portNumber}");
            int index = portList.FindIndex(e => e.desktopPort == portNumber || e.webPort == portNumber);

            portList[index].ClosePort(); // Ensure the port is actually closed
            portList.RemoveAt(index); // Remove from active ports

            Debug.Log($"Port {entry.desktopPort} & {entry.webPort} is closed");
            return entry;
        }

        Debug.LogWarning($"Port {portNumber} is not found in the active port list.");
        return null;
    }

    /// Not Used yet
    public bool OpenPort(int port)
    {
        PortEntry entry = portList.Find(e => e.desktopPort == port || e.webPort == port);
        if (entry.playerName != string.Empty)
        {
            entry.OpenPort();
            return true;
        }
        return false;
    }
    #endregion SERVER
}

[Serializable]
public struct PortEntry 
{
    public int desktopPort;
    public int webPort;
    public string playerName;
    public bool isOpen;


    public PortEntry(int desktopPort, int webPort, string playerName)
    {
        this.desktopPort = desktopPort;
        this.webPort = webPort;
        this.playerName = playerName;
        isOpen = false;
    }

    public void OpenPort() => isOpen = true;
    public void ClosePort() => isOpen = false;
}