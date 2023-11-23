using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportNavigation : MonoBehaviour
{
    public InputActionProperty teleportAction;

    public Transform head;
    public Transform hand;

    public LayerMask groundLayerMask;

    public GameObject previewAvatar;
    public GameObject hitpoint;

    public GameObject navigationPlatformGeometry;

    public float rayLength = 10.0f;
    private bool rayIsActive = false;

    public XRInteractorLineVisual lineVisual;
    private float rayActivationThreshhold = 0.01f;
    private float teleportActivationThreshhold = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        lineVisual.enabled = false;
        hitpoint.SetActive(false);
        previewAvatar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        /****************
         * Exercise 2.8 *
         ****************/

        // Read the current force applied to the teleporting hand.
        float teleportActionValue = teleportAction.action.ReadValue<float>();

        // If there's no input but the ray is active, just hide everything related to teleport.
        if (teleportActionValue < rayActivationThreshhold && rayIsActive)
        {
            ChangeRayVisibility(false);
        }

        // If there is an input value but the ray is inactive, show everything related to teleport.
        else if (teleportActionValue > rayActivationThreshhold && !rayIsActive)
        {
            ChangeRayVisibility(true);
        }

        if (rayIsActive)
        {
            Collider navigationPlatformGeometryCollider = navigationPlatformGeometry.GetComponent<Collider>();
            Collider lineVisualCollider = lineVisual.GetComponent<Collider>();

            // Check if the ray collides with the platform.
            if (navigationPlatformGeometryCollider.bounds.Intersects(lineVisualCollider.bounds))
            {
                // TODO: We need to find out how to get the collision vector to put the hitpoint there.
            }
        }

        // Exercise 2.8 Teleport Navigation
        // implement teleport states and functions like e.g.: SetHitpoint(), ShowPreviewAvatar(), PerformTeleport()
        // if (...) {
        // ...
        // hitpoint.transform.position = ... set hitpoint position
        // ...
        // }
    }

    void ChangeRayVisibility(bool isVisible)
    {
        rayIsActive = isVisible;
        lineVisual.enabled = isVisible;
        hitpoint.SetActive(isVisible);
    }
}
