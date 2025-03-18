using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using FXSceneEditor;
using UnityEngine.SceneManagement;

public class JoinPortRoomButtonView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private int portDesktopNumber;
    [SerializeField] private int portWebNumber;
    [SerializeField] private PortRoomSelectionView portRoomSelectionView;

    /// <summary>
    /// Setup button item
    /// </summary>
    public void Setup(string roomName, int portNumber, int portWeb, PortRoomSelectionView selectionView)
    {
        text.text = roomName;
        portDesktopNumber = portNumber;
        portWebNumber = portWeb;

        portRoomSelectionView = selectionView;
    }

    public void JoinRoom()
    {
        if (portRoomSelectionView != null)
        {
            portRoomSelectionView.JoinRoom(portDesktopNumber, portWebNumber);
        }
    }
}
