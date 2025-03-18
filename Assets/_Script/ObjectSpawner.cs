using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectSpawner : NetworkBehaviour
{
    public GameObject pickablePrefab;

    // Server-side method to spawn the object
    [Server]
    public async void SpawnObject(Vector3 spawnPosition)
    {
        if (pickablePrefab == null)
        {
            Debug.LogError("Pickable prefab is not assigned!");
            return;
        }

        GameObject spawnedObject = Instantiate(pickablePrefab, spawnPosition, Quaternion.identity);

        SceneManager.MoveGameObjectToScene(spawnedObject, SceneManager.GetSceneByBuildIndex(3));

        await UniTask.WaitForEndOfFrame();

        NetworkServer.Spawn(spawnedObject); // Spawn the object on all clients
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Spawning server object");
        SpawnObject(new Vector3(2.926056f, 0, -18.00968f));
    }
}
