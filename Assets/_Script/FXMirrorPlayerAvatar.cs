using Cinemachine;
using Mirror;
using StarterAssets;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FXMirrorPlayerAvatar : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraRoot;
    [SerializeField] private ThirdPersonController tpController;
    [SerializeField] private StarterAssetsInputs assetsInputs;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private List<GameObject> avatars;

    [SyncVar(hook = "OnAvatarIndexChanged")] public int avatarIndex;
    private CinemachineVirtualCamera player3POVCam;

    #region Server

    [Command]
    public void CmdChangeAvatarIndex(int newavatarIndex)
    {
        avatarIndex = newavatarIndex;
        ShowAvatar();
    }

    #endregion

    #region Client
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isOwned)
        {
            Debug.Log($"{netIdentity.netId} Avatar: is not yours");
            return;
        }

        // Setup Avatar
        avatarIndex = PlayerDataHandler.Instance.avatarIndex;
        ShowAvatar(); // Show local avatar
        CmdChangeAvatarIndex(avatarIndex); // Request to update server avatar

        // Find Player Virtual Camera
        GameObject vCamObj = GameObject.FindGameObjectWithTag("VCamThirdPOV");
        player3POVCam = vCamObj.GetComponent<CinemachineVirtualCamera>();

        // Set Camera Follow
        player3POVCam.Follow = playerCameraRoot;
        Debug.Log("Setup Camera follow");

        if (tpController != null)
        {
            tpController.enabled = true;
            Debug.Log("Enabled movementScript");
        }

        if (assetsInputs != null) 
        {
            assetsInputs.enabled = true;
            Debug.Log("Enabled assetsInputs");
        } 

        if (playerInput != null)
        {
            playerInput.enabled = true;
            Debug.Log("Enabled playerInput");
        }

        // Set interaction
        if (interactionController == null)
        {
            return;
        }

        interactionController.enabled = true;
        interactionController.Setup(transform);

        InteractionControllerView interactionView = FindAnyObjectByType<InteractionControllerView>();
        interactionView.interactionController = interactionController;
        interactionView.Setup();
    }

    public void OnAvatarIndexChanged(int oldValue, int newValue)
    {
        avatarIndex = newValue;
        ShowAvatar();
    }

    /// <summary>
    /// Show the character based on selected index
    /// </summary>
    private void ShowAvatar()
    {
        if (avatarIndex >= 0 && avatarIndex < avatars.Count)
        {
            DeactivateAllAvatars();
            avatars[avatarIndex].SetActive(true);
        }
    }

    /// <summary>
    /// Reset all character
    /// </summary>
    private void DeactivateAllAvatars()
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            avatars[i].SetActive(false);
        }
    }

    #endregion
}
