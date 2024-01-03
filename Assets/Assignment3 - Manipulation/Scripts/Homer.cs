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
    public LayerMask layerMask; // use this mask to raycast only for interactable objects

    [Header("Input Actions")]
    public InputActionProperty grabAction;

    [Header("Grab Configuration")]
    public HandCollider handCollider;

    // grab calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;

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

    // variables needed for hand offset calculation
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

        GrabCalculation();
    }

    #endregion

    #region Custom Methods

    private void DrawRay ()
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
        //TODO: your solution for excercise 3.5
        // use this function to calculate and adjust the hand as described in the h.o.m.e.r. technique
    }

    private void GrabCalculation()
    {
        // TODO: your solution for excercise 3.5
        // use this function to calculate the grabbing of an object

        if (Physics.Raycast(Origin, Direction, out hit, rayMaxLength, layerMask) && grabbedObject == null)
        {
            // Make the ray yellow if there is a hit.
            ray.startColor = Color.yellow;
            ray.endColor = Color.yellow;

            if (grabAction.action.WasPressedThisFrame())
            {
                // Make the ray yellow if there is a hit.
                ray.startColor = Color.red;
                ray.endColor = Color.red;

                // Store a reference to the hit object (the cube).
                grabbedObject = hit.collider.gameObject;
            }
        } else if (grabAction.action.WasReleasedThisFrame())
        {
            // If there's no hit, just recolor the ray again, back to white.
            ray.startColor = Color.white;
            ray.endColor = Color.white;

            // And reset the grabbed object reference.
            grabbedObject = null;
        } else if (grabbedObject == null)
        {
            // If there's no grabbed object, just recolor the ray again, back to white.
            ray.startColor = Color.white;
            ray.endColor = Color.white;
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
