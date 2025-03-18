using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    public float updateDistance = 50f; // Only update if within this distance

    private float distanceToCamera;
    private Transform mainCameraTransform;

    private void Start()
    {
        // Cache the main camera transform for better performance
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No Main Camera found. Ensure your camera has the 'MainCamera' tag.");
        }
    }

    private void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        // Check object distance from camera
        distanceToCamera = Vector3.Distance(transform.position, mainCameraTransform.position);

        // Check if canvas is within the camera's field of view
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        bool isVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (isVisible && distanceToCamera <= updateDistance)
        {
            Vector3 direction = mainCameraTransform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(-direction);
            transform.rotation = rotation;
        }
    }
}
