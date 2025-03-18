using UnityEngine;
using UnityEngine.Events;

public class InteractableToggle : Interactable
{
    public bool isOn = false; // Stores the toggle state

    [Header("Toggle Events")]
    public UnityEvent OnToggleOn; // Called when isOn becomes true
    public UnityEvent OnToggleOff; // Called when isOn becomes false

    public override void TriggerInteraction()
    {
        isOn = !isOn; // Toggle the boolean value

        if (isOn)
        {
            OnToggleOn?.Invoke(); // Trigger On event
        }
        else
        {
            OnToggleOff?.Invoke(); // Trigger Off event
        }

        base.TriggerInteraction(); // Call the base interaction event
        Debug.Log($"{gameObject.name} toggled to: {isOn}");

        // Ensure the object remains interactable
        base.TriggerInteraction();
    }
}
