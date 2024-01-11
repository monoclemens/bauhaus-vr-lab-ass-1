using Unity.Netcode;
using UnityEngine;

public class ManipulationSelector : NetworkBehaviour
{
    #region Member Variables

    private NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    #endregion

    #region Selector Methods

    public bool RequestGrab()
    {
        Debug.Log("RequestGrab!");

        // TODO: your solution for excercise 3.8
        // check if object can be grabbed by a user
        // trigger ownership handling
        // trigger grabbed state update

        // If it is already grabbed, we don't permit grabbing it. Return false.
        if (isGrabbed.Value == true)
        {
            return false;
        }

        ChangeOwnershipServerRpc();
        UpdateGrabbedStateServerRpc();

        // Not sure about this return statement. I suppose it's correct, but it seems too simple.
        return true;
    }

    public void Release()
    {
        Debug.Log("Release!");

        // TODO: your solution for excercise 3.8
        // use this function trigger a grabbed state update on object release

        UpdateGrabbedStateServerRpc();
    }

    #endregion

    #region RPCs

    // TODO: your solution for excercise 3.8
    // implement a rpc to transfer the ownership of an object 
    // implement a rpc to update the isGrabbed value

    /**
     * A ServerRpc for changing ownership. We need to pass the parameter RequireOwnership as false,
     * because any client must be able to execute it. No matter if they already own the network object or not.
     * 
     * It returns the owning client's ID or null if ownership was removed.
     * 
     * Docs on ServerRpc: https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc/
     */
    [ServerRpc(RequireOwnership = false)]
    public void ChangeOwnershipServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("ChangeOwnershipServerRpc!");

        ulong clientId = serverRpcParams.Receive.SenderClientId;

        Debug.Log("clientID: " + clientId);

        NetworkObject thisNetworkObject = GetComponent<NetworkObject>();

        Debug.Log("current owner id: " + thisNetworkObject.OwnerClientId);

        thisNetworkObject.ChangeOwnership(clientId);

        Debug.Log("owner id after ChangeOwnershipServerRpc: " + thisNetworkObject.OwnerClientId);
    }

    /**
     * A ServerRpc for updating the grabbed state. We don't need to pass the parameter RequireOwnership,
     * because only the owning client must be able to execute it.
     * 
     * It returns the new grabbed state's bool.
     */
    [ServerRpc]
    public void UpdateGrabbedStateServerRpc()
    {
        Debug.Log("UpdateGrabbedStateServerRpc!");
        
        var nextIsGrabbed = !isGrabbed.Value;

        Debug.Log("nextIsGrabbed: " + nextIsGrabbed);

        isGrabbed.Value = nextIsGrabbed;

        // If the client released the object, remove their ownership.
        if (nextIsGrabbed == false)
        {
            Debug.Log("Next bool is false!");

            NetworkObject thisNetworkObject = GetComponent<NetworkObject>();
            thisNetworkObject.RemoveOwnership();

            Debug.Log("owner id after UpdateGrabbedStateServerRpc: " + thisNetworkObject.OwnerClientId);
        }


    }

    #endregion
}
