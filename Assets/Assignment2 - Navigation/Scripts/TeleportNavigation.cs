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
    public GameObject hitpoint;

    public GameObject navigationPlatformGeometry;

    public float rayLength = 10.0f;
    private bool rayIsActive = false;

    public XRInteractorLineVisual lineVisual;
    private float rayActivationThreshhold = 0.01f;
    private float teleportActivationThreshhold = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        lineVisual.enabled = false;
        hitpoint.SetActive(false);
        previewAvatar.SetActive(false);
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
            hitpoint.SetActive(true); // show
        }
        else if (teleportActionValue < rayActivationThreshhold && rayIsActive)
        {
            rayIsActive = false;
            lineVisual.enabled = rayIsActive;
            hitpoint.SetActive(false); // hide
        }

        // Exercise 2.8 Teleport Navigation
        // implement teleport states and functions like e.g.: SetHitpoint(), ShowPreviewAvatar(), PerformTeleport()
        // if (...) {
        // ...
        // hitpoint.transform.position = ... set hitpoint position
        // ...
        // }
    }


}   
