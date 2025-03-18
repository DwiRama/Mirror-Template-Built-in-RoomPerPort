using kcp2k;
using Mirror;
using TMPro;
using UnityEngine;

public class NetworkPanelView : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TMP_InputField inputField_networkAddress;
    [SerializeField] private TMP_InputField inputField_port;

    private void Start()
    {
        if (networkManager == null)
            networkManager = NetworkManager.singleton;

        inputField_networkAddress.text = networkManager.networkAddress;

        var transport = Transport.active as PortTransport;
        inputField_port.text = transport.Port.ToString();
    }

    public void OnInputNetworkAddressChanged()
    {
        if (networkManager == null)
        {
            Debug.Log("Network Manager not found");
            return;
        }

        networkManager.networkAddress = inputField_networkAddress.text;
    }

    public void OnInputPortChanged()
    {
        if (networkManager == null)
        {
            Debug.Log("Network Manager not found");
            return;
        }

        var transport = Transport.active as PortTransport;
        if (transport != null && ushort.TryParse(inputField_port.text, out ushort port))
        {
            transport.Port = port;
        }
    }
}
