using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Mirror;
using UnityEngine;

namespace FXSceneEditor
{
    public class SessionHandler_NetworkBehaviour : NetworkBehaviour
    {
        public string executableFileName
        {
            get => System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        }

        [Client]
        public void CreateLocalSession(string portNumber, string portWeb, string startSceneName, string guestName, string scenarioKey, string ipNetwork = "localhost", bool isMainProcess = false, bool isServer = true)
        {
            StartChildProcess("-ismainprocess \"" + isMainProcess + "\"", "-isserver \"" + isServer + "\"", "-portnumber \"" + portNumber + "\"", "-portweb \"" + portWeb + "\"", "-ipnetwork \"" + ipNetwork + "\"", "-startscenename \"" + startSceneName + "\"", "-guestname \"" + guestName + "\"", "-scenariokey \"" + scenarioKey + "\"");
        }

        [Command(requiresAuthority = false)]
        protected void StartChildProcess(params string[] args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = executableFileName,
                // startInfo.FileName = "D:/Test/Learn/003-Mirror/[Learn] - Mirror/Build/Learn Mirror_0.0.3/[Learn] - Mirror.exe";
                Arguments = string.Join(' ', args),
                CreateNoWindow = true,
                UseShellExecute = true
            };
            UnityEngine.Debug.Log("Arguments : " + startInfo.Arguments);

            Process process = Process.Start(startInfo);
        }

        [TargetRpc]
        public void TargetGetRandomAvailablePort(NetworkConnectionToClient networkConnectionToClient, int scenarioItemIndex, int port)
        {
            ScenarioHandler.Instance.GetRandomAvailablePort(scenarioItemIndex, port);
        }

        [Command(requiresAuthority = false)]
        public void CmdGetRandomAvailablePort(GameObject player, int scenarioItemIndex, int min, int max, int maxTries)
        {
            var _conn = player.GetComponent<NetworkIdentity>();
            TargetGetRandomAvailablePort(_conn.connectionToClient, scenarioItemIndex, PortHandler.GetRandomAvailablePort(min, max, maxTries));
        }
    }
}
