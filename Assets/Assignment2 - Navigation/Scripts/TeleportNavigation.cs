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
    //added to activate the rotation selection for the previewavatar to start
    public bool previewAvatarPlaced = false;

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
        if (rayIsActive)
        {
            Debug.Log("Showing the ray...");

            Vector3 hitPosition = SetHitPoint();

            if (isValueGreaterThanTeleportThreshold)
            {

                ShowPreviewAvatar(hitPosition);
            }
            else
            {
               
                previewAvatar.SetActive(false);
                previewAvatarPlaced = false;

            }

            


        }
        //need to deactivate when ray is inactive cause it floats needlessly otherwise
        else {
           
           DeactivateHitPoint();
            
        }

        // Exercise 2.8 Teleport Navigation
        // implement teleport states and functions like e.g.: SetHitpoint(), ShowPreviewAvatar(), PerformTeleport()
        // if (...) {
        // ...
        // hitpoint.transform.position = ... set hitpoint position
        // ...
        // }
    }

    //sets the hit point and returns where it is for the preview to be set 
    //not sure if it is the prettiest way to do it
    private Vector3 SetHitPoint()
    {
        /** 
        * Put the hitpoint where the ray collided with the ground layer mask.
        * If there is no collision, deactivate the hitpoint so it doesn't float anywhere needlessly.
        */
        Vector3 origin = hand.position;
        Vector3 direction = hand.forward;

        Debug.Log("Calculating a hitpoint with the layer mask...");
        bool isHittingLayerMask = Physics.Raycast(origin, direction, out RaycastHit hitInfo, rayLength, groundLayerMask);

        if (isHittingLayerMask)
        {

            ActivateHitPoint();
            hitpoint.transform.position = hitInfo.point;
        }
        else
        {
            Debug.Log("No hit found.");
            DeactivateHitPoint();
        }

        return hitInfo.point;
    }

    private void ShowPreviewAvatar(Vector3 hitPosition)
    {
       
            //place the preview avatar if not previously placed
        if (!previewAvatarPlaced)
        {
            Vector3 previewAvatarPosition = hitPosition;
            previewAvatarPosition.y = head.position.y;
            previewAvatar.SetActive(true);
            previewAvatar.transform.position = previewAvatarPosition;
            // Set the initial rotation to match the head object
            previewAvatar.transform.rotation = head.rotation;
            previewAvatarPlaced = true;
        }
        else
        {
            // Calculate the rotation to face towards the hitpoint
            Quaternion lookRotation = Quaternion.LookRotation(previewAvatar.transform.position - hitPosition, Vector3.up);

            // Lock rotation to only rotate around the y-axis
            Quaternion yRotationOnly = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
            previewAvatar.transform.rotation = yRotationOnly;
        }


    
        
    }
    //doesn't work properly yet
    //TODO: where should the function be placed is the question
    private void PerformTeleport()
    {
        transform.position = previewAvatar.transform.position;
        transform.rotation = previewAvatar.transform.rotation;
    }
    private void ToggleRay() => ChangeRayVisibility(!rayIsActive);

    private void ChangeRayVisibility(bool isVisible)
    {
        rayIsActive = isVisible;
        lineVisual.enabled = isVisible;
        
    }
    //prettier looking activation setters
    private void DeactivateHitPoint()
    {
        hitpoint.SetActive(false);
    }
    private void ActivateHitPoint()
    {
        hitpoint.SetActive(true);
    }
}


/*lock the preview avatar in place as soon as we exceed the teleport threshold
                if (isValueGreaterThanTeleportThreshold && !previewAvatarPlaced)
                {
                    Vector3 previewAvatarPosition = hitInfo.point;
                    previewAvatarPosition.y = head.position.y;
                    previewAvatar.SetActive(true);
                    previewAvatar.transform.position = previewAvatarPosition;
                    previewAvatar.SetActive(false);
                    previewAvatarPlaced = true;
                }
                else if (!isValueGreaterThanTeleportThreshold)
                {
                    previewAvatarPlaced = false;

                }*/