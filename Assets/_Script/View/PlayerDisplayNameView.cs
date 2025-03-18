using Mirror;
using TMPro;
using UnityEngine;

public class PlayerDisplayNameView : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerDisplayName;

    [SyncVar(hook = "OnDisplayNameChange")] public string displayName;
    private FXMirrorPlayerInstance playerInstance;

    #region Server

    [Command]
    public void CmdChangeDisplayName(string newName)
    {
        displayName = newName;
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        if (!isOwned)
            return;

        // Get PlayerInstance
        playerInstance = NetworkClient.localPlayer.GetComponent<FXMirrorPlayerInstance>();
        UpdateDisplayName();

        //playerInstance.OnPlayerChangedName += UpdateDisplayName;
    }

    //public override void OnStopClient()
    //{
    //    if (isOwned && playerInstance != null)
    //    {
    //        playerInstance.OnPlayerChangedName -= UpdateDisplayName;
    //    }

    //    base.OnStopClient();
    //}

    /// <summary>
    /// Update local display name and request to update to server
    /// </summary>
    public void UpdateDisplayName()
    {
        if (playerInstance == null || playerDisplayName == null)
            return;

        playerDisplayName.text = PlayerDataHandler.Instance.playerName;

        // Request server to change the display name
        CmdChangeDisplayName(playerDisplayName.text);
    }

    //public void UpdateDisplayName(string newName)
    //{
    //    if (playerInstance == null || playerDisplayName == null)
    //        return;

    //    playerDisplayName.text = newName;

    //    // Request server to change the display name
    //    CmdChangeDisplayName(playerDisplayName.text);
    //}

    /// <summary>
    /// Hook called on client when playerName is change on the server
    /// </summary>
    /// <param name="oldName">previous playerName value</param>
    /// <param name="newName">new playerName value</param>
    public void OnDisplayNameChange(string oldName, string newName)
    {
        displayName = newName;
        playerDisplayName.text = displayName;
        Debug.Log($"{netIdentity.netId} change name to {displayName}");
    }

    #endregion
}
