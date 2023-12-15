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

    // calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;

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
        // TODO: your solution for excercise 3.4
        // use this function to implement an object-grabbing that re-parents the object to the hand without snapping
        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && handCollider.isColliding && CanGrab)
            {
                grabbedObject = handCollider.collidingObject;
                //set the parent
                grabbedObject.transform.SetParent(transform);
            }

        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
                grabbedObject.GetComponent<ManipulationSelector>().Release();
            //reset the parent to null
            grabbedObject.transform.SetParent(null);
            grabbedObject = null;

        }

    }
    //it snaps for some reason  even though we take into account the position offset
    private void CalculationGrab()
    {
        // TODO: your solution for excercise 3.4
        // use this function to implement an object-grabbing that uses an offset calculation without snapping (and no re-parenting!)
        if (grabAction.action.IsPressed())
        {

            if (grabbedObject == null && handCollider.isColliding && CanGrab)
            {
                grabbedObject = handCollider.collidingObject;
                //get the initial offset from the object to the hand
                offsetMatrix = GetTransformationMatrix(transform, true).inverse * GetTransformationMatrix(grabbedObject.transform, true);
            }
            if (grabbedObject != null)
            {
                //calculate the new transformation based on initial offset and the changing hand position
                Matrix4x4 newTransformationMatrix = GetTransformationMatrix(transform, true) * offsetMatrix;
                //get the translation column and rotation and set them
                grabbedObject.transform.position = newTransformationMatrix.GetColumn(3);
                grabbedObject.transform.rotation = Quaternion.LookRotation(newTransformationMatrix.GetColumn(2), newTransformationMatrix.GetColumn(1));

            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
                grabbedObject.GetComponent<ManipulationSelector>().Release();
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
                return false;

            bool isAllowedToGrab = handCollider
                .collidingObject
                .GetComponent<ManipulationSelector>()
                .RequestGrab();

            return isAllowedToGrab;
        }
    }

    #endregion
}
