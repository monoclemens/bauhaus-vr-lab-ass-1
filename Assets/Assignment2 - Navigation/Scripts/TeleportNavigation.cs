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
    //public GameObject previewPlatform;
    public GameObject hitpoint;

    public GameObject navigationPlatformGeometry;

    public float rayLength = 10.0f;

    private bool rayIsActive = false;
    private bool previewIsActive = false;

    private Vector3 currentHitPoint;
    private Vector3 targetPoint;

    public XRInteractorLineVisual lineVisual;
    private LineRenderer lineRenderer;
    private float rayActivationThreshhold = 0.01f;
    private float teleportActivationThreshhold = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        lineVisual.enabled = false;
        hitpoint.SetActive(false);
        previewAvatar.SetActive(false);
        //previewPlatform.SetActive(false);

<<<<<<< Updated upstream
    // This method only figures out in which state the user is.
    private UserState CalculateNextUserState()
    {
        // Read the current force applied to the teleporting hand.
        float teleportActionValue = teleportAction.action.ReadValue<float>();

        bool isValueGreaterThanRayThreshold = teleportActionValue > rayActivationThreshhold;
        bool isValueGreaterThanTeleportThreshold = teleportActionValue > teleportActivationThreshhold;

        var (isHittingLayerMask, _) = CalculateHitPointCollision();

        // If the user just crossed the ray threshold, we should be casting the ray.
        if (userState == UserState.Idle && isValueGreaterThanRayThreshold)
        {
            Debug.Log("New state: CastingRay");

            return UserState.CastingRay;
        }
        // If the user crossed the teleport threshold AND if there is a hit point collision, show the preview avatar.
        else if (userState == UserState.CastingRay && isValueGreaterThanTeleportThreshold && isHittingLayerMask)
        {
            Debug.Log("New state: AdjustingAvatar");

            return UserState.AdjustingAvatar;
        }
        // If the user is adjusting the teleport but the force becomes smaller than the teleport threshold, they released the trigger.
        else if (userState == UserState.AdjustingAvatar && !isValueGreaterThanTeleportThreshold)
        {
            Debug.Log("New state: ReleasedTrigger");

            return UserState.ReleasedTrigger;
        }
        // If the user released the trigger and the force is smaller than the ray threshold, we should go back to idling.
        else if (userState == UserState.ReleasedTrigger && !isValueGreaterThanRayThreshold)
        {
            Debug.Log("New state: Idle");

            return UserState.Idle;
        }
        // Otherwise nothing important changed. Just return the current value.
        return userState;
=======
        //if (lineRenderer == null)
        //{
        //    lineRenderer = GetComponent<LineRenderer>();
        //}
>>>>>>> Stashed changes
    }

    // Update is called once per frame
    void Update()
    {
        // activate line
        float teleportActionValue = teleportAction.action.ReadValue<float>();
        if (teleportActionValue > rayActivationThreshhold && !rayIsActive)
        {
            rayIsActive = true;
            lineVisual.enabled = rayIsActive;
        }
        else if (teleportActionValue < rayActivationThreshhold && rayIsActive)
        {
            rayIsActive = false;
            lineVisual.enabled = rayIsActive;
        }
<<<<<<< Updated upstream
        else
        {
            Debug.Log("No hit point found.");
        }
=======

        if (rayIsActive)
        {
            if (Physics.Raycast(hand.position, hand.forward * rayLength, out RaycastHit hit, 10f, groundLayerMask))
            {
                currentHitPoint = hit.point;
                Debug.Log("hit:" + hit.transform.name);
                ShowHitpoint(currentHitPoint);
                if (teleportActionValue > teleportActivationThreshhold && !previewIsActive)
                {
                    previewIsActive = true;
                    SetTeleportTarget(currentHitPoint);
>>>>>>> Stashed changes

                    // Show Avatar
                    float userHeight = head.transform.position.y - this.transform.position.y;
                    Quaternion avatarOrientation = Quaternion.LookRotation(head.transform.position - new Vector3(currentHitPoint.x, head.transform.position.y, currentHitPoint.z));
                    ShowPreview(currentHitPoint + new Vector3(0, userHeight, 0), avatarOrientation);
                }
                else if (previewIsActive) // jump action active -> update
                {
                    // Update Avatar
                    float userHeight = head.position.y - this.transform.position.y;
                    Vector3 previewAvatarPosition = new Vector3(previewAvatar.transform.position.x, currentHitPoint.y + userHeight, previewAvatar.transform.position.z);
                    Quaternion avatarOrientation = Quaternion.LookRotation(previewAvatar.transform.position - new Vector3(currentHitPoint.x, previewAvatar.transform.position.y, currentHitPoint.z));
                    ShowPreview(previewAvatarPosition, avatarOrientation);
                }
                //else
                //{
                //    ShowHitpoint(currentHitPoint);
                //}
            }
            else
            {
                HideHitpoint();
                HidePreview();
            }        
        }
        else
        {
            HideHitpoint();
            HidePreview();
        }

        // jump action triggered and released -> perform jump
        if (previewIsActive && teleportActionValue < teleportActivationThreshhold)
        {
            PerformJump();
            previewIsActive = false;

            HideHitpoint();
            HidePreview();
        }
    }

    private void PerformJump()
    {
        Quaternion goalOrientation = Quaternion.LookRotation(new Vector3(currentHitPoint.x, previewAvatar.transform.position.y, currentHitPoint.z) - previewAvatar.transform.position);
        Vector3 goalRotY = new Vector3(0f, goalOrientation.eulerAngles.y, 0f);
        Matrix4x4 goalMat = Matrix4x4.TRS(targetPoint, Quaternion.Euler(goalRotY), new Vector3(1, 1, 1));

        Vector3 headYRot = new Vector3(0f, head.transform.localRotation.eulerAngles.y, 0f);
        Vector3 headXZPos = new Vector3(head.transform.localPosition.x, 0f, head.transform.localPosition.z);
        Matrix4x4 headMat = Matrix4x4.TRS(headXZPos, Quaternion.Euler(headYRot), new Vector3(1, 1, 1));

        Matrix4x4 newMat = goalMat * Matrix4x4.Inverse(headMat);

        transform.position = newMat.GetColumn(3);
        transform.rotation = newMat.rotation;
        transform.localScale = newMat.lossyScale;
    }

    private void ShowHitpoint(Vector3 worldPos)
    {
        hitpoint.SetActive(true); // show
        hitpoint.transform.position = worldPos;
    }

    private void HideHitpoint()
    {
<<<<<<< Updated upstream
        // Get a matrix representation of the preview avatar.
        var previewAvatarMatrix = previewAvatar.transform.localToWorldMatrix;

        // And one of the head.
        var localHeadMatrix = Matrix4x4.TRS(
            head.localPosition,
            head.localRotation,
            head.localScale
        );

        // To get the new place, multiply the preview avatar's matrix with the inverse of our head's matrix.
        // This way the local offsets are cancelled out of the new place.
        var newPlaceMatrix = previewAvatarMatrix * localHeadMatrix.inverse;

        // Set the position and scale straight away.
        transform.position = newPlaceMatrix.GetColumn(3);
        transform.localScale = newPlaceMatrix.lossyScale;

        // Had to inverse the y rotation because the avatar models face opposite directions.
        transform.rotation = newPlaceMatrix.rotation * Quaternion.AngleAxis(180, Vector3.up);

        // Remove all axis rotations but the y axis.
        Quaternion newRotation = transform.rotation;
        newRotation.eulerAngles = new Vector3(0, newRotation.eulerAngles.y, 0);
        transform.rotation = newRotation;
=======
        hitpoint.SetActive(false); // hide
>>>>>>> Stashed changes
    }

    private void ShowPreview(Vector3 worldPos, Quaternion worldRot)
    {
        previewAvatar.SetActive(true); // show
        previewAvatar.transform.position = worldPos;
        previewAvatar.transform.rotation = worldRot;
    }

    private void HidePreview()
    {
        previewAvatar.SetActive(false); // hide
    }

    private void SetTeleportTarget(Vector3 targetPos)
    {
        targetPoint = targetPos;
    }
}   
