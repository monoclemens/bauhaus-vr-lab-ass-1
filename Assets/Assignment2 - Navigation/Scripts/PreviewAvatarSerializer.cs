using Unity.Netcode;
using UnityEngine;

public class PreviewAvatarSerializer : NetworkBehaviour
{
    public GameObject previewAvatar;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        previewAvatar.SetActive(false);

        if (IsOwner)
            return;
       
    }


    private void Update()
    {
        if (IsOwner)
        {
            
        }
        else if (!IsOwner)
        {
            
        }
    }

}
