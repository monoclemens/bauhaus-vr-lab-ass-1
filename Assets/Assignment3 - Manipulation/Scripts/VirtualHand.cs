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

    private bool grabbed = false;

    private bool canGrab
    {
        get
        {
            if (handCollider.isColliding)
                return handCollider.collidingObject.GetComponent<ManipulationSelector>().RequestGrab();
            return false;
        }
    }

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        if(GetComponentInParent<NetworkObject>() != null)
            if (!GetComponentInParent<NetworkObject>().IsOwner)
            {
                Destroy(this);
                return;
            }
    }

    private void Update()
    {
        if (toggleAction.action.WasPressedThisFrame())
        {
            grabMethod = (VirtualHandsMethod)(((int)grabMethod + 1) % 3);
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

    private void SnapGrab()
    {
        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
            }

            if (grabbedObject != null)
            {
                grabbedObject.transform.position = transform.position;
                grabbedObject.transform.rotation = transform.rotation;
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if(grabbedObject != null)
                grabbedObject.GetComponent<ManipulationSelector>().Release();
            grabbedObject = null;
        }
    }

    private void ReparentingGrab()
    {
        // TODO: your solution for excercise 3.4
        // use this function to implement an object-grabbing that re-parents the object to the hand without snapping
        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
            }
            //set the parent if not previously set
            if (grabbedObject != null && grabbedObject.transform.parent == null)
            {
                grabbedObject.transform.SetParent(transform);
                Vector3 offset = transform.InverseTransformPoint(grabbedObject.transform.position);
                Debug.Log(transform.position.ToString());
                Debug.Log(offset.ToString());
            }
            
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
                grabbedObject.GetComponent<ManipulationSelector>().Release();
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
            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
            }
            if (grabbedObject != null && !grabbed)
            {
                Matrix4x4 handMatrix = GetTransformationMatrix(transform);
                Matrix4x4 grabbedObjectMatrix = GetTransformationMatrix(grabbedObject.transform);

                // Calculate the offset matrix by multiplying the inverse of thisObjectMatrix with grabbedObjectMatrix
                Matrix4x4 offsetMatrix = handMatrix.inverse * grabbedObjectMatrix;
                grabbed = true;

                Vector3 offset = offsetMatrix.GetColumn(3);
                Debug.Log(transform.position.ToString());
                Debug.Log(offset.ToString());
                
            }
            if (grabbedObject != null && grabbed)
            {
                // Extract the position from the offsetMatrix
                Vector3 offset = offsetMatrix.GetColumn(3);
                Vector3 newPosition = transform.position + offset;

                Quaternion offsetRotation = Quaternion.LookRotation(
                offsetMatrix.GetColumn(2),
                offsetMatrix.GetColumn(1));
                Quaternion newRotation = transform.rotation * offsetRotation;

                grabbedObject.transform.position = newPosition;
                
            }

        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
                grabbedObject.GetComponent<ManipulationSelector>().Release();
            grabbedObject = null;
            grabbed = false;

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

    #endregion
}
