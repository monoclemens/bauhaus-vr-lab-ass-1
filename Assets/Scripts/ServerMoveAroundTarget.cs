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
        Vector3 upVector = Vector3.up;  // The axis around which to rotate

        // Calculate the rotation angle for the current frame
        float rotationAngle = degreesPerSecond * Time.deltaTime;

        // Calculate the position of the object relative to the target
        Vector3 relativePosition = transform.position - target.transform.position;

        // Rotate the relative position around the target using Quaternion
        Quaternion rotation = Quaternion.AngleAxis(rotationAngle, upVector);
        relativePosition = rotation * relativePosition;

        Vector3 newPosition = target.transform.position + relativePosition;

        return newPosition;
    }

    

  

    Quaternion CalculateRotationUpdate(Vector3 newPosition)
    {
        // Your code for Exercise 1.2 here
        Vector3 moveDirection = newPosition - transform.position;
        moveDirection.y = 0; // Ignore the y-component
        transform.rotation = Quaternion.LookRotation(moveDirection);


        return transform.rotation;
    }
}
