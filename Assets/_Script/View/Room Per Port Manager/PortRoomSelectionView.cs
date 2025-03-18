using Cysharp.Threading.Tasks;
using FXSceneEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortRoomSelectionView : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private GameObject roomButtonPrefab;
    [SerializeField] private GameObject fakeloading;
    private FXMirrorPlayerLobbyInstance playerLobbyInstance;

    public void Setup(FXMirrorPlayerLobbyInstance newPlayerLobbyInstance)
    {
        playerLobbyInstance = newPlayerLobbyInstance;

        if (playerLobbyInstance != null)
        {
            playerLobbyInstance.OnOpenPortsUpdate += OnOpenPortsUpdate;
        }
    }

    private void OnDestroy()
    {
        if (playerLobbyInstance != null)
        {
            playerLobbyInstance.OnOpenPortsUpdate -= OnOpenPortsUpdate;
        }
    }

    /// <summary>
    /// Refresh port list
    /// </summary>
    /// <param name="openEntries"></param>
    private void OnOpenPortsUpdate(List<PortEntry> openEntries)
    {
        int buttonCount = container.childCount;

        // Ensure we have enough buttons
        for (int i = 0; i < openEntries.Count; i++)
        {
            GameObject button;

            if (i < buttonCount)
            {
                // Reuse existing button
                button = container.GetChild(i).gameObject;
                button.SetActive(true);
            }
            else
            {
                // Create new button if needed
                button = Instantiate(roomButtonPrefab, container);
            }

            // Enable button only if the port is open
            button.SetActive(openEntries[i].isOpen);

            // Update button text with player name and port
            var buttonView = button.GetComponent<JoinPortRoomButtonView>();
            if (buttonView != null)
            {
                buttonView.Setup($"{openEntries[i].playerName}", openEntries[i].desktopPort, openEntries[i].webPort, this);
            }
        }

        // Deactivate extra buttons
        for (int i = openEntries.Count; i < buttonCount; i++)
        {
            container.GetChild(i).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Join Port Room
    /// </summary>
    /// <param name="portDesktopNumber">available desktop port</param>
    /// <param name="portWebNumber">available web port</param>
    public async void JoinRoom(int portDesktopNumber, int portWebNumber)
    {
        if (portDesktopNumber == -1 || portWebNumber == -1)
        {
            return;
        }

        fakeloading.SetActive(true);

        StartupSceneHandler.PortNumber = portDesktopNumber;
        StartupSceneHandler.PortWeb = portWebNumber;

        StartupSceneHandler.IsMainProcess = false;
        await UniTask.WaitForSeconds(2);
        SceneManager.LoadScene(2);
        Debug.Log("Scene loaded: " + SceneManager.GetSceneByBuildIndex(2).name);
    }
}
