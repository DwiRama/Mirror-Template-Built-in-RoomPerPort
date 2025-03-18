using UnityEngine;
using TMPro;

public class PlayerNameView : MonoBehaviour
{
    public TMP_InputField inputField_playerName;

    private void Start()
    {
        inputField_playerName.text = PlayerDataHandler.Instance.playerName;
    }

    public void OnInputPlayerNameChanged()
    {
        PlayerDataHandler.Instance.playerName = inputField_playerName.text;
    }
}
