using Unity.Netcode;
using UnityEngine;

public class ServerTimedRotate : NetworkBehaviour
{
    public float degreesPerSecondX = 0;
    public float degreesPerSecondY = 20;
    public float degreesPerSecondZ = 0;

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;

        // Your code for Exercise 1.4 here 
        Vector3 rotation = new(
            degreesPerSecondX, 
            degreesPerSecondY, 
            degreesPerSecondZ);

        // Ensure that frame rates don't affect the rotation speed by including deltatime in the equation.
        Vector3 normalizedRotation = rotation * Time.deltaTime;
        
        // Around the world, baby!
        transform.Rotate(normalizedRotation, Space.World);
    }
}
