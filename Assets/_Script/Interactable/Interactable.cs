using Mirror;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : NetworkBehaviour
{
    [Header("Interaction Events")]
    public UnityEvent OnLookAt; // Called when first looked at
    public UnityEvent OnInteract; // Called when "E" is pressed
    public UnityEvent OnInteractionComplete; // Called when interaction is finished

    [Header("Setting")]
    public bool waitforNotifyInteractionComplete = false;

    [Space(30)]

    private bool hasBeenLookedAt = false;
    private bool isInteracting = false;

    #region Server

    [Server]
    public void ServerHandleOnLook(uint requesterNetId)
    {
        if (NetworkServer.spawned.TryGetValue(requesterNetId, out NetworkIdentity netIdentity))
        {
            Debug.Log(gameObject.name + " ServerHandleOnLook called by " + netIdentity.GetComponent<FXMirrorPlayerInstance>().playerName);
        }

        if (IsValidLookAt())
        {
            TriggerLookAt();
            RpcTriggerLookAt(requesterNetId);
        }
    }

    [Server]
    bool IsValidLookAt()
    {
        return true; // Add validation logic here
    }

    [Server]
    public void ServerHandleInteraction()
    {
        Debug.Log(gameObject.name + " ServerHandleInteraction called by ");
        // Validate the interaction (e.g., distance checks, cooldowns)
        if (IsValidInteraction())
        {
            TriggerInteraction();
            RpcTriggerInteraction();
        }
    }

    [Server]
    bool IsValidInteraction()
    {
        if (!isInteracting) 
        {
            isInteracting = true;
        }
        else
        {
            isInteracting = false;
        }

        return isInteracting;
    }

    #endregion

    #region Client

    [ClientRpc]
    void RpcTriggerLookAt(uint requesterNetId)
    {
        TriggerLookAt();

        if (NetworkClient.spawned.TryGetValue(requesterNetId, out NetworkIdentity netIdentity))
        {
            Debug.Log("LookAt triggered for all clients triggered by " + netIdentity.GetComponent<FXMirrorPlayerInstance>().playerName);
        }
    }

    [ClientRpc]
    void RpcTriggerInteraction()
    {
        TriggerInteraction();
        Debug.Log("Interaction triggered for all clients!");
    }

    #endregion

    public virtual void TriggerLookAt()
    {
        if (!hasBeenLookedAt)
        {
            OnLookAt?.Invoke();
            hasBeenLookedAt = true;
        }
    }

    public virtual void TriggerInteraction()
    {
        OnInteract?.Invoke();
        isInteracting = false;
    }

    public virtual void TriggerInteractionComplete()
    {
        OnInteractionComplete?.Invoke();
    }
}
