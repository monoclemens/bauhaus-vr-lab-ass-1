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

        transform.SetPositionAndRotation(newPosition, newRotation);
    }

    Vector3 CalculatePositionUpdate()
    {
        // The axis around which to rotate.
        Vector3 upVector = Vector3.up;

        // Calculate the rotation angle for the current frame.
        float rotationAngle = degreesPerSecond * Time.deltaTime;

        // Rotate the relative position around the target using Quaternion.
        Quaternion rotation = Quaternion.AngleAxis(rotationAngle, upVector);

        // Calculate the position of the object relative to the target.
        Vector3 relativePosition = transform.position - target.transform.position;

        // Add rotation to the equation.
        Vector3 rotatedRelativePosition = rotation * relativePosition;

        Vector3 newPosition = target.transform.position + rotatedRelativePosition;

        return newPosition;
    }

    Quaternion CalculateRotationUpdate(Vector3 newPosition)
    {
        // Your code for Exercise 1.2 here
        Vector3 moveDirection = newPosition - transform.position;

        // Ignore the y-component
        moveDirection.y = 0;

        return Quaternion.LookRotation(moveDirection);
    }
}
