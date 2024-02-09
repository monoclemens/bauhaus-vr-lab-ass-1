using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GoGo : MonoBehaviour
{
    #region Member Variables

    [Header("Go-Go Components")] 
    public Transform head;
    public float originHeadOffset = 0.2f;
    public Transform hand;

    [Header("Go-Go Parameters")] 
    public float distanceThreshold;
    [Range(0, 1)] public float k;
    
    [Header("Input Actions")] 
    public InputActionProperty grabAction;
    
    [Header("Grab Configuration")]
    public HandCollider handCollider;
    
    // calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;
    
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
        ApplyHandOffset();
        GrabCalculation();
    }

    #endregion

    #region Custom Methods

    private void ApplyHandOffset()
    {
        Vector3 origin = head.position;
        origin.y -= originHeadOffset;

        float distance = Vector3.Distance(origin, hand.position);

        if (distance < distanceThreshold)
        {
            transform.position = hand.position;
        }
        else
        {
            Vector3 direction = hand.position - origin;
            
            float offsetDistance = k * Mathf.Pow((direction.magnitude - distanceThreshold) * 100, 2);
            offsetDistance = direction.magnitude + offsetDistance / 100;
            
            transform.position = origin + direction.normalized * offsetDistance;
        }
        
        transform.rotation = hand.rotation;
    }

    private void GrabCalculation()
    {
        if (grabAction.action.WasPressedThisFrame())
        {
            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
                offsetMatrix = GetTransformationMatrix(transform, true).inverse *
                               GetTransformationMatrix(grabbedObject.transform, true);
            }
        }
        else if (grabAction.action.IsPressed())
        {
            if (grabbedObject != null)
            {
                Matrix4x4 newTransform = GetTransformationMatrix(transform, true) * offsetMatrix;

                grabbedObject.transform.position = newTransform.GetColumn(3);
                grabbedObject.transform.rotation = newTransform.rotation;
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if(grabbedObject != null)
                grabbedObject.GetComponent<ManipulationSelector>().Release();
            grabbedObject = null;
            offsetMatrix = Matrix4x4.identity;
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
