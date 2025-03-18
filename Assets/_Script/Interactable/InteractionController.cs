using UnityEngine;
using UnityEngine.Events;
using System;
using Mirror;

public class InteractionController : NetworkBehaviour
{
    [Header("Settings")]
    public Transform interactionPoint; // Assign a position (e.g., hand or chest area)
    public float interactionRange = 3f;
    public LayerMask interactableLayer;

    private Camera playerCamera;
    private Interactable currentInteractable;
    private bool isInteracting = false; // Track interaction state

    public UnityEvent<Interactable> OnLookAt;
    public UnityEvent OnInteract;
    public UnityEvent OnInteractionComplete;
    public UnityEvent OnLookAway;

    public void Setup(Transform interactionPoint)
    {
        playerCamera = Camera.main;
        this.interactionPoint = interactionPoint;
    }

    void Update()
    {
        if (isOwned && !isInteracting && playerCamera != null) // Only allow interaction if not already interacting
        {
            TryRaycast();

            if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
            {
                TryInteract();
            }
        }
    }
    #region Server

    [Command]
    void CmdRequestOnLook(uint objectNetId, uint playerNetId)
    {
        if (NetworkServer.spawned.TryGetValue(objectNetId, out NetworkIdentity obj))
        {
            Interactable interactable = obj.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.ServerHandleOnLook(playerNetId);
            }
        }
    }

    [Command]
    void CmdRequestInteraction(uint objectNetId)
    {
        if (NetworkServer.spawned.TryGetValue(objectNetId, out NetworkIdentity obj))
        {
            Interactable interactable = obj.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.ServerHandleInteraction();
            }
        }
    }

    #endregion

    #region Local
    public void TryRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(interactionPoint.position, transform.forward, out hit, interactionRange, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null && interactable != currentInteractable)
            {
                currentInteractable = interactable;
                CmdRequestOnLook(currentInteractable.netId, FXMirrorPlayerInstance.localPlayer.netId);
                OnLookAt?.Invoke(interactable); // Notify UI
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                OnLookAway?.Invoke(); // Call event when looking away
                currentInteractable = null;
            }
        }
    }

    public void TryInteract()
    {
        if (currentInteractable != null)
        {
            Debug.Log("Interacting with object: " + currentInteractable.name + " | netId: " + currentInteractable.netId);

            if (currentInteractable != null && currentInteractable.waitforNotifyInteractionComplete)
            {
                isInteracting = true; // Disable further interactions
            }

            CmdRequestInteraction(currentInteractable.netId);
            OnInteract?.Invoke();

            if (currentInteractable != null && !currentInteractable.waitforNotifyInteractionComplete)
            {
                OnInteractionComplete?.Invoke();
            }
        }
    }
    public void NotifyInteractionComplete()
    {
        if (isOwned)
        {
            isInteracting = false; // Re-enable interactions
            OnInteractionComplete?.Invoke();
        }
    }

    #endregion

}
