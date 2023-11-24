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
    private readonly float rayActivationThreshhold = 0.01f;
    private readonly float teleportActivationThreshhold = 0.5f;

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

        bool isValueGreaterThanRayThreshold = teleportActionValue > rayActivationThreshhold;
        bool isValueGreaterThanTeleportThreshold = teleportActionValue > teleportActivationThreshhold;

        /**
         * If the value is greater than the ray threshold, the ray should be active and vice versa.
         * If that's not the case, toggle the ray on/off.
         */
        if (isValueGreaterThanRayThreshold != rayIsActive)
        {
            Debug.Log("Toggling the ray " + (rayIsActive ? "off..." : "on..."));

            ToggleRay();
        }

        /**
         * If the ray is active but we're not adjusting the teleport yet, 
         * show the hitpoint where the ray hits the layer mask.
         */
        if (rayIsActive && !isValueGreaterThanTeleportThreshold)
        {
            Debug.Log("Showing the ray...");

            Vector3 origin = lineVisual.transform.localPosition;
            Vector3 direction = lineVisual.transform.rotation.eulerAngles;

            Debug.Log("Calculating a hitpoint with the layer mask...");

            bool isHittingLayerMask = Physics.Raycast(origin, direction, out RaycastHit hitInfo, rayLength, groundLayerMask);

            /** 
             * Put the hitpoint where the ray collided with the ground layer mask.
             * If there is no collision, deactivate the hitpoint so it doesn't float anywhere needlessly.
             */
            if (isHittingLayerMask)
            {
                Debug.Log("Hit found: " + hitInfo.ToString());

                hitpoint.transform.position = hitInfo.point;
            }
            else
            {
                Debug.Log("No hit found.");

                hitpoint.SetActive(false);
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


    private void ToggleRay() => ChangeRayVisibility(!rayIsActive);

    private void ChangeRayVisibility(bool isVisible)
    {
        rayIsActive = isVisible;
        lineVisual.enabled = isVisible;
        hitpoint.SetActive(isVisible);
    }
}
