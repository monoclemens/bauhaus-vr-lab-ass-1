using Unity.Netcode;
using UnityEngine;

public class ServerMoveAroundTarget : NetworkBehaviour
{
    public Transform target;

    public float degreesPerSecond = 20;

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;
        var newPosition = CalculatePositionUpdate();
        var newRotation = CalculateRotationUpdate(newPosition);
        transform.position = newPosition;
        transform.rotation = newRotation;
    }

    Vector3 CalculatePositionUpdate()
    {
        // Your code for Exercise 1.2 here
        return transform.position;
    }

    Quaternion CalculateRotationUpdate(Vector3 newPosition)
    {
        // Your code for Exercise 1.2 here
        return transform.rotation;
    }
}
