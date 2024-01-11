using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

enum GrabState
{
    Idle, Hit, Grab
}

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
    public LayerMask layerMask; // use this mask to raycast only for interactable objects

    [Header("Input Actions")]
    public InputActionProperty grabAction;

    [Header("Grab Configuration")]
    public HandCollider handCollider;

    // grab calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;

    // Added by Clay to keep track of the grab state.
    private GrabState grabState = GrabState.Idle;

    // utility bool to check if you can grab an object
    private bool CanGrab
    {
        get
        {
            if (handCollider.isColliding)
            {
                return handCollider.collidingObject.GetComponent<ManipulationSelector>().RequestGrab();
            }

            return false;
        }
    }

    /**
     * Variables needed for hand offset calculation.
     * 
     * grabOffsetDistance refers to the distance between origin and object.
     * grabHandDistance refers to the distance between origin and hand.
     */
    private RaycastHit hit;
    private float grabOffsetDistance;
    private float grabHandDistance;

    // convenience variables for hand offset calculations
    private Vector3 Origin
    {
        get
        {
            Vector3 v = head.position;
            v.y -= originHeadOffset;
            return v;
        }
    }

    private Vector3 Direction => hand.position - Origin;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        ray.enabled = enabled;
    }

    private void Start()
    {
        if (GetComponentInParent<NetworkObject>() != null)
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

        UpdateRay();
        GrabCalculation();
    }

    #endregion

    #region Custom Methods

    private void DrawRay()
    {
        var positions = new Vector3[2];

        positions[0] = Origin;
        positions[1] = Direction * rayMaxLength;

        ray.SetPositions(positions);
    }

    private void UpdateRay()
    {
        // TODO: your solution for excercise 3.5
        // use this function to calculate and adjust the ray of the h.o.m.e.r. technique

        DrawRay();
    }

    private void ApplyHandOffset()
    {
        // TODO: your solution for excercise 3.5
        // use this function to calculate and adjust the hand as described in the h.o.m.e.r. technique

        // Compute the current distance between origin and tracked hand.
        float currentHandOriginDistance = Vector3.Distance(Origin, hand.transform.position);

        // Compute the relation between this current distance and the initial distance, which was stored when grabbing the object.
        float relativeHandBodyDistance = currentHandOriginDistance / grabHandDistance;

        // Now use that relation to compute the current distance between origin and object.
        float currentObjectOriginDistance = grabOffsetDistance * relativeHandBodyDistance;

        Ray tempRay = new Ray(Origin, Direction);
        var newPosition = tempRay.GetPoint(currentObjectOriginDistance);

        transform.position = newPosition;
    }

    private GrabState ComputeGrabState()
    {
        // If there is a grabbed object already and if the action button is pressed, just keep grabbing.
        if (grabbedObject != null && grabAction.action.IsPressed())
        {
            return GrabState.Grab;
        }

        // If we release the object, reset its parent and remove the reference to it.
        if (grabbedObject != null && grabAction.action.WasReleasedThisFrame())
        {
            grabbedObject.transform.SetParent(null);
            grabbedObject = null;
        }

        // Check if there is a hit.
        if (Physics.Raycast(Origin, Direction, out hit, rayMaxLength, layerMask))
        {
            // If the action is pressed, the user is grabbing the object. 
            if (grabAction.action.WasPressedThisFrame())
            {
                // Move the virtual hand to the object.
                transform.position = hit.point;

                return GrabState.Grab;
            }

            // Otherwise there is only a hit.
            return GrabState.Hit;
        }

        return GrabState.Idle;
    }

    private void ColorRay(GrabState grabState)
    {
        switch (grabState)
        {
            case GrabState.Idle:
                ray.startColor = Color.white;
                ray.endColor = Color.white;
                break;
            case GrabState.Hit:
                ray.startColor = Color.green;
                ray.endColor = Color.green;
                break;
            case GrabState.Grab:
                ray.startColor = Color.blue;
                ray.endColor = Color.blue;
                break;
        }
    }

    private void GrabCalculation()
    {
        // TODO: your solution for excercise 3.5
        // use this function to calculate the grabbing of an object

        // Get the current state of the grab.
        grabState = ComputeGrabState();

        // Color the ray accordingly.
        ColorRay(grabState);

        if (grabState == GrabState.Grab)
        {
            if (grabbedObject == null)
            {
                grabbedObject = hit.collider.gameObject;
                grabbedObject.transform.SetParent(transform);
            }

            /**
             * If we just grabbed the object, remember the distance between 
             *      - the user's origin and the hit point &
             *      - the user's origin and their hand.
             */
            if (grabAction.action.WasPressedThisFrame())
            {
                grabOffsetDistance = hit.distance;
                grabHandDistance = Vector3.Distance(Origin, hand.transform.position);
            }
        }
        else
        {
            if (grabbedObject != null)
            {
                grabbedObject.GetComponent<ManipulationSelector>().Release();
                grabbedObject.transform.SetParent(null);
            }

            grabbedObject = null;
            transform.position = hand.position;
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
