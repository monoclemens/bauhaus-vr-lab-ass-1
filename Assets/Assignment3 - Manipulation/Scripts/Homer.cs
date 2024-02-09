using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Homer : MonoBehaviour
{
    #region Member Variables

    [Header("H.O.M.E.R. Components")] 
    public Transform head;
    public float originHeadOffset = 0.2f;
    public Transform hand;

    [Header("H.O.M.E.R. Parameters")] 
    public LineRenderer ray;
    public float rayMaxLength = 100f;
    public LayerMask layerMask;
    
    [Header("Input Actions")] 
    public InputActionProperty grabAction;

    [Header("Grab Configuration")]
    public HandCollider handCollider;

    // grab calculation variables
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
    
    // variables needed for hand offset calculation
    private RaycastHit hit;
    private float grabOffsetDistance;
    private float grabHandDistance;
    
    // convenience variables for hand offset calculations
    private Vector3 origin
    {
        get
        {
            Vector3 v = head.position;
            v.y -= originHeadOffset;
            return v;
        }
    }
    private Vector3 direction => hand.position - origin;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        ray.enabled = enabled;
    }

    private void Start()
    {
        if(GetComponentInParent<NetworkObject>() != null)
            if (!GetComponentInParent<NetworkObject>().IsOwner)
            {
                Destroy(this);
                return;
            }

        ray.positionCount = 2;
    }

    private void Update()
    {
        if (grabbedObject == null)
            UpdateRay();
        else
            ApplyHandOffset();

        GrabCalculation();
    }

    #endregion

    #region Custom Methods

    private void UpdateRay()
    {
        if (Physics.Raycast(origin, direction, out hit, rayMaxLength, layerMask))
        {
            ray.SetPosition(0, origin);
            ray.SetPosition(1, hit.point);
            
            ray.startColor = Color.green;
            ray.endColor = Color.green;

            handCollider.transform.position = hit.point;
        }
        else
        {
            ray.SetPosition(0, origin);
            ray.SetPosition(1, hand.position + direction.normalized * rayMaxLength);
            
            ray.startColor = Color.red;
            ray.endColor = Color.red;

            handCollider.transform.position = hand.position;
        }
    }

    private void ApplyHandOffset()
    {
        float offsetFactor = Vector3.Distance(origin, hand.position) / grabHandDistance;
        float offsetDistance = grabOffsetDistance * offsetFactor;
        
        transform.position = origin + direction.normalized * offsetDistance;
        transform.rotation = hand.rotation;
    }

    private void GrabCalculation()
    {
        if (grabAction.action.WasPressedThisFrame())
        {
            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
                offsetMatrix = GetTransformationMatrix(handCollider.transform, true).inverse *
                               GetTransformationMatrix(grabbedObject.transform, true);
                
                grabOffsetDistance = Vector3.Distance(origin, hit.point);
                grabHandDistance = Vector3.Distance(origin, hand.position);

                ray.enabled = false;
                handCollider.transform.position = hand.position;
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
            
            transform.localPosition = Vector3.zero;
            ray.enabled = true;
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
