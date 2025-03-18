using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterSelectionHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> avatars;
    private int selectedCharacter;

    private void Start()
    {
        selectedCharacter = 0;
        ShowCharacter(selectedCharacter);
    }

    public void NextCharacter()
    {
        selectedCharacter++;
        if (selectedCharacter >= avatars.Count)
        {
            selectedCharacter = 0;
        }

        ShowCharacter(selectedCharacter);
    }

    public void PrevCharacter()
    {
        selectedCharacter--;
        if (selectedCharacter < 0)
        {
            selectedCharacter = avatars.Count - 1;
        }

        ShowCharacter(selectedCharacter);
    }

    public void ShowCharacter(int index) 
    {
        if (index >= 0 && index < avatars.Count)
        {
            DeactivateAllAvatars();
            avatars[index].SetActive(true);

            if (PlayerDataHandler.Instance != null)
            {
                PlayerDataHandler.Instance.avatarIndex = index;
            }
        }
    }

    private void DeactivateAllAvatars()
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            avatars[i].SetActive(false);
        }
    }
}
