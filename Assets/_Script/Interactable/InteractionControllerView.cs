using UnityEngine;
using TMPro;

public class InteractionControllerView : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject interactionPrompt; // UI for "Press E to interact"
    public TextMeshProUGUI interactionText; // Optional: Customizable text

    public InteractionController interactionController;

    void Start()
    {
        if (interactionController != null)
        {
            interactionController.OnLookAt.AddListener(ShowInteractionPrompt);
            //interactionController.OnInteract.AddListener(HideInteractionPrompt);
            //interactionController.OnInteractionComplete.AddListener(HideInteractionPrompt);
            interactionController.OnLookAway.AddListener(HideInteractionPrompt); // Subscribe here!
        }

        interactionPrompt.SetActive(false); // Hide by default
    }

    void OnDestroy()
    {
        if (interactionController != null)
        {
            interactionController.OnLookAt.RemoveListener(ShowInteractionPrompt);
            //interactionController.OnInteract.RemoveListener(HideInteractionPrompt);
            //interactionController.OnInteractionComplete.RemoveListener(HideInteractionPrompt);
            interactionController.OnLookAway.RemoveListener(HideInteractionPrompt); // Subscribe here!
        }
    }

    public void Setup()
    {
        if (interactionController != null)
        {
            interactionController.OnLookAt.AddListener(ShowInteractionPrompt);
            //interactionController.OnInteract.AddListener(HideInteractionPrompt);
            //interactionController.OnInteractionComplete.AddListener(HideInteractionPrompt);
            interactionController.OnLookAway.AddListener(HideInteractionPrompt); // Subscribe here!
        }

        interactionPrompt.SetActive(false); // Hide by default
    }

    public void StartInteraction()
    {
        if (interactionController != null)
        {
            interactionController.TryInteract();
        }
    }

    public void ShowInteractionPrompt(Interactable interactable)
    {
        interactionPrompt.SetActive(true);
        //if (interactionText != null)
        //{
        //    interactionText.text = $"Press E to interact with {interactable.gameObject.name}";
        //}
    }

    public void HideInteractionPrompt()
    {
        interactionPrompt.SetActive(false);
    }
}
