using Unity.Netcode;

public class ManipulationSelector : NetworkBehaviour
{
    #region Member Variables

    private NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    #endregion

    #region Selector Methods

    public bool RequestGrab()
    {
        if (!isGrabbed.Value)
        {
            if (!IsOwner)
                TransferOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);

            SetIsGrabbedServerRpc(true);
            return true;
        }

        return false;
    }

    public void Release()
    {
        SetIsGrabbedServerRpc(false);
    }

    #endregion

    #region RPCs

    [ServerRpc(RequireOwnership = false)]
    private void TransferOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetIsGrabbedServerRpc(bool grabbed)
    {
        isGrabbed.Value = grabbed;
    }

    #endregion
}
