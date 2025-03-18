using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FXSceneEditor
{
    public class SessionHandler : MonoBehaviourSingleton<SessionHandler>
    {
        public static SessionHandler_NetworkBehaviour SessionHandlerNetworkBehaviour;
        [SerializeField] SessionHandler_NetworkBehaviour networkBehaviour_SessionHandler;

        // Start is called before the first frame update
        void Start()
        {
            networkBehaviour_SessionHandler = FindAnyObjectByType<SessionHandler_NetworkBehaviour>(FindObjectsInactive.Include);
            SessionHandlerNetworkBehaviour = networkBehaviour_SessionHandler;
        }

        public void CreateLocalSession(string portNumber, string portWeb, string startSceneName, string guestName, string scenarioKey, string ipNetwork = "localhost", bool isMainProcess = false, bool isServer = true)
        {
            networkBehaviour_SessionHandler.CreateLocalSession(portNumber, portWeb, startSceneName, guestName, scenarioKey, ipNetwork, isMainProcess, isServer);
        }
    }
}
