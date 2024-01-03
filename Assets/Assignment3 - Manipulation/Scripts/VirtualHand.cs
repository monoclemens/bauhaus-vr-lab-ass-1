using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualHand : MonoBehaviour
{
    #region Member Variables

    private enum VirtualHandsMethod
    {
        Snap,
        Reparenting,
        Calculation
    }

    [Header("Input Actions")]
    public InputActionProperty grabAction;
    public InputActionProperty toggleAction;

    [Header("Configuration")]
    [SerializeField] private VirtualHandsMethod grabMethod;
    public HandCollider handCollider;

    // Calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;

    private Vector3 globalInitialHandPosition;
    private Quaternion globalInitialHandRotation;

    private Vector3 globalInitialObjectPosition;
    private Quaternion globalInitialObjectRotation;

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        var networkObject = GetComponentInParent<NetworkObject>();

        // If there is a network object but we're not the owner, destroy the VirtualHand.
        if (networkObject != null && networkObject.IsOwner == false)
        {
            Destroy(this);

            return;
        }
    }

    private void Update()
    {
        if (toggleAction.action.WasPressedThisFrame())
        {
            int nextMethodIndex = (int)grabMethod + 1;

            // Apply modulo to make sure the index gets wrapped around to 0 once it hits 3.
            int remainder = nextMethodIndex % 3;

            grabMethod = (VirtualHandsMethod)(remainder);
        }

        switch (grabMethod)
        {
            case VirtualHandsMethod.Snap:
                SnapGrab();
                break;
            case VirtualHandsMethod.Reparenting:
                ReparentingGrab();
                break;
            case VirtualHandsMethod.Calculation:
                CalculationGrab();
                break;
        }
    }

    #endregion

    #region Grab Methods

    /**
     * Simple grabbing via snapping. This uses the hand only.
     * 
     * If the user is grabbing, either store the newly grabbed and colliding (!) object (if available)
     * or update the transform of the already grabbed object.
     * 
     * If the user just released the grab, release the colliding object if there is one. 
     * Set the grabbed object to null anyway.
     */
    private void SnapGrab()
    {
        Debug.Log("SnapGrab");

        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && handCollider.isColliding && CanGrab)
            {
                grabbedObject = handCollider.collidingObject;
            }

            if (grabbedObject != null)
            {
                grabbedObject.transform.SetPositionAndRotation(
                    transform.position,
                    transform.rotation
                );
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
            {
                grabbedObject.GetComponent<ManipulationSelector>().Release();
            }

            grabbedObject = null;
        }
    }

    private void ReparentingGrab()
    {
        Debug.Log("ReparentingGrab");

        // TODO: Your solution for excercise 3.4.
        // Use this function to implement an object-grabbing that re-parents the object to the hand without snapping.
        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && handCollider.isColliding && CanGrab)
            {
                grabbedObject = handCollider.collidingObject;

                // Set the parent.
                // TODO: Is it really this simple?
                grabbedObject.transform.SetParent(transform);
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null) 
            { 
                grabbedObject.GetComponent<ManipulationSelector>().Release(); 
            }

            // Reset the parent to null.
            // TODO: Will this cause issues with the object's transform? Does Unity do the heavy lifting once again?
            grabbedObject.transform.SetParent(null);
            grabbedObject = null;
        }
    }

    private void CalculationGrab()
    {
        Debug.Log("CalculationGrab");

        // TODO: your solution for excercise 3.4.
        // Use this function to implement an object-grabbing that uses an offset calculation without snapping (and no re-parenting!)
        if (grabAction.action.IsPressed())
        {
            // Initial grab if there is no grabbed object yet.
            if (grabbedObject == null && handCollider.isColliding && CanGrab)
            {
                grabbedObject = handCollider.collidingObject;

                // Store the hand's current transform.
                globalInitialHandPosition = transform.position;
                globalInitialHandRotation = transform.rotation;

                // Store the object's initial transform, too.
                globalInitialObjectPosition = grabbedObject.transform.position;
                globalInitialObjectRotation = grabbedObject.transform.rotation;

                // Get the hand's global transform and inverse it.
                var globalHandTransformationMatrix = GetTransformationMatrix(transform, true);
                var inverseGlobalHandTransformationMatrix = globalHandTransformationMatrix.inverse;

                // Get the grabbed object's global transform.
                var globalGrabbedObjectTransformationMatrix = GetTransformationMatrix(grabbedObject.transform, true);

                // Multiply both transform matrixes to get the initial offset matrix.
                offsetMatrix = globalGrabbedObjectTransformationMatrix * inverseGlobalHandTransformationMatrix;
            }

            // Calculate the grabbed object's transform whenever there is a grabbed object.
            if (grabbedObject != null)
            {
                var globalHandTransformPositionDelta = transform.position - globalInitialHandPosition;
                var globalHandTransformRotationDelta = transform.rotation * Quaternion.Inverse(globalInitialHandRotation);

                // Get both the translation column and rotation and set them.
                grabbedObject.transform.SetPositionAndRotation(
                    globalInitialObjectPosition + globalHandTransformPositionDelta, 
                    globalInitialObjectRotation * globalHandTransformRotationDelta
                );
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
            {
                grabbedObject.GetComponent<ManipulationSelector>().Release();
            }

            grabbedObject = null;
        }
    }

    #endregion

    #region Utility Functions

    public Matrix4x4 GetTransformationMatrix(Transform t, bool inWorldSpace = true)
    {
        if (inWorldSpace)
        {
            return Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        }
        else
        {
            return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
        }
    }

    private bool CanGrab
    {
        get
        {
            if (handCollider.isColliding == false)
            {
                return false;
            }

            bool isAllowedToGrab = handCollider
                .collidingObject
                .GetComponent<ManipulationSelector>()
                .RequestGrab();

            return isAllowedToGrab;
        }
    }

    #endregion
}
