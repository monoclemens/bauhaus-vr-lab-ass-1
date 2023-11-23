using Unity.Netcode;
using UnityEngine;

public class ServerMoveAroundParent : NetworkBehaviour
{
    public float degreesPerSecond = 20;
    
    private Vector3 localPositionXZ
    {
        get
        {
            return new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }
    }
    
    Vector3 CalculatePositionUpdate()
    {
        var y = transform.localPosition.y;
        var newPosition = RotationUtils.ManualYRotation(localPositionXZ, degreesPerSecond * Time.deltaTime);
        newPosition.y = y;
        return newPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;
        transform.localPosition = CalculatePositionUpdate();
    }
}
