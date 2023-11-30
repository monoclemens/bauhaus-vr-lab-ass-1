using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

enum UserState
{
    Idle, // Nothing.
    CastingRay, // Only showing the ray.
    AdjustingAvatar, // Showing both the ray and the preview avatar.
    ReleasedTrigger, // The state between going from AdjustingAvatar to Idle.
}

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

    private UserState userState = UserState.Idle;

    // Start is called before the first frame update
    void Start()
    {
        HideAll();
    }

    // This method just hides anything of relevance.
    private void HideAll()
    {
        lineVisual.enabled = false;
        hitpoint.SetActive(false);
        previewAvatar.SetActive(false);
    }

    // This method only figures out in which state the user is.
    private UserState CalculateNextUserState()
    {
        // Read the current force applied to the teleporting hand.
        float teleportActionValue = teleportAction.action.ReadValue<float>();

        bool isValueGreaterThanRayThreshold = teleportActionValue > rayActivationThreshhold;
        bool isValueGreaterThanTeleportThreshold = teleportActionValue > teleportActivationThreshhold;

        var (isHittingLayerMask, _) = CalculateHitPointCollision();

        Debug.Log("User state is casting ray: " + (userState == UserState.CastingRay));
        Debug.Log(isValueGreaterThanTeleportThreshold);
        Debug.Log(isHittingLayerMask);

        // If the user just crossed the ray threshold, we should be casting the ray.
        if (userState == UserState.Idle && isValueGreaterThanRayThreshold)
        {
            return UserState.CastingRay;
        }
        // If the user crossed the teleport threshold AND if there is a hit point collision, show the preview avatar.
        else if (userState == UserState.CastingRay && isValueGreaterThanTeleportThreshold && isHittingLayerMask)
        {
            return UserState.AdjustingAvatar;
        }
        // If the user is adjusting the teleport but the force becomes smaller than the teleport threshold, they released the trigger.
        else if (userState == UserState.AdjustingAvatar && !isValueGreaterThanTeleportThreshold)
        {
            return UserState.ReleasedTrigger;
        }
        // If the user released the trigger and the force is smaller than the ray threshold, we should go back to idling.
        else if (userState == UserState.ReleasedTrigger && !isValueGreaterThanRayThreshold)
        {
            return UserState.Idle;
        }
        // Otherwise nothing important changed. Just return the current value.
        return userState;
    }

    // Update is called once per frame
    void Update()
    {
        /****************
         * Exercise 2.8 *
         ****************/

        userState = CalculateNextUserState();

        switch (userState)
        {
            // If we're idling, hide the ray, the hit point and the preview avatar.
            case UserState.Idle:
                HideAll();

                break;
            // If we're casting the ray, activate the ray, but only if it's inactive.
            // Then, show the hit point where it collides with the layer mask, but only if it does collide with the layer mask.
            case UserState.CastingRay:
                ActivateInactiveRay();

                ShowCollidingHitPoint();

                break;
            // If we're adjusting the avatar, start showing the avatar and change its rotation according to the ray's hit point.
            // We'll ignore the ray because it should already be active.
            case UserState.AdjustingAvatar:
                ShowCollidingHitPoint();

                var (_, hitInfo) = CalculateHitPointCollision();

                if (previewAvatarPlaced == false)
                {
                    SetPreviewAvatar(hitInfo.point);

                    previewAvatarPlaced = true;
                }
                else
                {
                    RotatePreviewAvatar(hitInfo.point);
                }

                break;
            /**
             * If the trigger is released:
             *      - move the user to the preview avatar's position
             *      - rotate the user to the preview avatar's rotation
             *      - reset previewAvatarPlaced
             *      - hide the ray, hit point and the preview avatar.
             */
            case UserState.ReleasedTrigger:
                PerformTeleport();

                previewAvatarPlaced = false;

                HideAll();

                break;

        }
    }

    private (bool isHittingLayerMask, RaycastHit hitInfo) CalculateHitPointCollision()
    {
        Debug.Log("Calculating a hitpoint with the layer mask...");

        Vector3 origin = hand.position;
        Vector3 direction = hand.forward;

        bool isHittingLayerMask = Physics.Raycast(
            origin,
            direction,
            out RaycastHit hitInfo,
            rayLength,
            groundLayerMask
        );

        if (isHittingLayerMask)
        {
            Debug.Log("Hit point found!");
        }
        else
        {
            Debug.Log("No hit point found.");
        }

        return (isHittingLayerMask, hitInfo);
    }

    private void ShowCollidingHitPoint()
    {
        var (isHittingLayerMask, hitInfo) = CalculateHitPointCollision();

        if (isHittingLayerMask)
        {
            hitpoint.transform.position = hitInfo.point;

            ActivateHitPoint();
        }
        else
        {
            DeactivateHitPoint();
        }
    }

    private void SetPreviewAvatar(Vector3 position)
    {
        Vector3 previewAvatarPosition = position;
        previewAvatarPosition.y = head.position.y;
        previewAvatar.SetActive(true);
        previewAvatar.transform.position = previewAvatarPosition;

        // Set the initial rotation to match the head object
        previewAvatar.transform.rotation = head.rotation;
    }

    private void RotatePreviewAvatar(Vector3 hitPointPosition)
    {
        // Calculate the rotation to face towards the hitpoint
        Quaternion lookRotation = Quaternion.LookRotation(previewAvatar.transform.position - hitPointPosition, Vector3.up);

        // Lock rotation to only rotate around the y-axis
        Quaternion yRotationOnly = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);

        previewAvatar.transform.rotation = yRotationOnly;
    }

    //doesn't work properly yet
    //TODO: where should the function be placed is the question
    private void PerformTeleport()
    {
        transform.position = previewAvatar.transform.position;
        transform.rotation = previewAvatar.transform.rotation;
    }

    private void ChangeRayVisibility(bool isVisible)
    {
        rayIsActive = isVisible;
        lineVisual.enabled = isVisible;
    }

    // This method activates the ray only if it is inactive.
    private void ActivateInactiveRay()
    {
        if (rayIsActive && lineVisual.enabled) return;

        ChangeRayVisibility(true);
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