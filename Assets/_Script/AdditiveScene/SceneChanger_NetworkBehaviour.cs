using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FXSceneEditor
{
    public class SceneChanger_NetworkBehaviour : NetworkBehaviour
    {
        [Scene][SerializeField] string _currentScene;
        public static SceneChanger_NetworkBehaviour Instance;

        void Start()
        {
            Instance = this;
            // DontDestroyOnLoad(this);
        }

        [Client]
        public void OnClickNextScene(string destinationScene)
        {
            var sceneName = AdditiveNetworkManagerFXM.NetworkedSceneDatas[destinationScene];
            CmdOnClickNextScene(sceneName, Constant.LocalPlayer);
        }

        [Command(requiresAuthority = false)]
        void CmdOnClickNextScene(string destinationScene, GameObject player)
        {
            StartCoroutine(SendPlayerToNewScene(destinationScene, player));
        }

        [Server]
        IEnumerator SendPlayerToNewScene(string destinationScene, GameObject player)
        {
            yield return new WaitForSeconds(1);
            if (player.TryGetComponent(out NetworkIdentity identity))
            {
                NetworkConnectionToClient conn = identity.connectionToClient;
                if (conn == null) yield break;

                // Tell client to unload previous subscene. No custom handling for this.
                conn.Send(new SceneMessage { sceneName = _currentScene, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });

                NetworkServer.RemovePlayerForConnection(conn, false);

                // // reposition player on server and client
                // player.transform.position = startPosition;
                // player.transform.LookAt(Vector3.up);

                // Move player to new subscene.
                SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(destinationScene));

                // Tell client to load the new subscene with custom handling (see NetworkManager::OnClientChangeScene).
                conn.Send(new SceneMessage { sceneName = destinationScene, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

                NetworkServer.AddPlayerForConnection(conn, player);
            }
        }
    }
}