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

        k = 0.9f;
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
        // TODO: your solution for excercise 3.6
        // use this function to calculate and apply the hand displacement according to the go-go technique

        float distanceDelta = TrackedHandOriginDistance - distanceThreshold;

        Debug.Log("Origin: " + Origin);
        Debug.Log("Distance: " + TrackedHandOriginDistance);
        Debug.Log("Delta: " + distanceDelta);
        Debug.Log("k: " + k);

        // Do nothing underneath the threshold.
        if (distanceDelta < 0)
        {
            return;
        }

        Debug.Log("Delta is over threshold!");

        float squaredDistanceDelta = distanceDelta * distanceDelta;

        float nonIsomorphicFactor = k * squaredDistanceDelta;

        Debug.Log("Factor: " + nonIsomorphicFactor + "; TrackedHandOriginDistance: " + TrackedHandOriginDistance);

        // Now move the virtual hand to where the tracked one is PLUS the additional distance.
        transform.position = hand.transform.position + hand.transform.position * nonIsomorphicFactor;
    }

    private void GrabCalculation()
    {
        // TODO: your solution for excercise 3.6
        // use this function to calculate the grabbing of an object
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

    private Vector3 Origin
    {
        get
        {
            var origin = head.position;
            origin.y -= originHeadOffset;

            return origin;
        }
    }

    // The distance between the TRACKED hand and the origin of the user.
    private float TrackedHandOriginDistance
    {
        get
        {
            var distance = Vector3.Distance(hand.position, Origin);

            return distance;
        }
    }
    #endregion
}
