using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayHitpointSerializer : NetworkBehaviour
{
    public GameObject hitpoint;
    public LineRenderer ray;
    public Transform hand;
    private NetworkVariable<bool> rayHitpointEnabled = new NetworkVariable<bool>(default, writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> hitpointPosition = new NetworkVariable<Vector3>(default, writePerm: NetworkVariableWritePermission.Owner);
 
    public override void OnNetworkSpawn()
    {
        hitpoint.SetActive(false);
        ray.positionCount = 2;
        ray.enabled = false;

        if (IsOwner)
            return;

        ApplyRayUpdates();
    }


    private bool SerializeRayUpdates(out bool rayEnabled, out Vector3 hitPos)
    {
        rayEnabled = false;
        hitPos = hitpoint.transform.position;
        if (hitpoint.activeSelf)
        {
            rayEnabled = true;
            return true;
        }
        else if (!hitpoint.activeSelf && rayHitpointEnabled.Value)
        {
            rayEnabled = false;
            return true;
        }

        return false;
    }

    private void ApplyRayUpdates()
    {
        hitpoint.SetActive(rayHitpointEnabled.Value);
        ray.enabled = rayHitpointEnabled.Value;
        if (hitpoint.activeSelf)
        {
            ray.SetPosition(0, hand.position);
            ray.SetPosition(1, hitpointPosition.Value);
            hitpoint.transform.position = hitpointPosition.Value;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (SerializeRayUpdates(out bool enabled, out Vector3 pos))
            {
                rayHitpointEnabled.Value = enabled;
                hitpointPosition.Value = pos;
            }
        }
        else if (!IsOwner)
        {
            ApplyRayUpdates();
        }
    }
}
