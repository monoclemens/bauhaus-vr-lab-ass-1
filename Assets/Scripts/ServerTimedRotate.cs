using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ServerTimedRotate : NetworkBehaviour
{
    public float degreesPerSecondX = 0;
    public float degreesPerSecondY = 20;
    public float degreesPerSecondZ = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;
        transform.localRotation *= Quaternion.Euler(
            degreesPerSecondX * Time.deltaTime,
            degreesPerSecondY * Time.deltaTime,
            degreesPerSecondZ * Time.deltaTime
        );    
    }
}
