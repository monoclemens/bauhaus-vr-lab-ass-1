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
        Vector3 rotation = new Vector3(degreesPerSecondX, degreesPerSecondY, degreesPerSecondZ) * Time.deltaTime;
        transform.Rotate(rotation, Space.World);
    }


}
