using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class StartResetButton : MonoBehaviour
{
    bool isPressed = false;
    GameObject pressingUser;

    public GameObject button;
    public GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        TriggerStartResetButtonServerRpc(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != pressingUser) return;

        button.transform.localPosition = new(0, 1f, 0);
        isPressed = false;
    }

    [ServerRpc(RequireOwnership = false)]
    void TriggerStartResetButtonServerRpc(Collider collider)
    {

        TriggerStartResetButtonClientRpc(collider);
    }

    [ClientRpc]
    void TriggerStartResetButtonClientRpc(Collider collider)
    {
        if (isPressed) return;
        isPressed = true;

        button.transform.localPosition = new(0, 0.95f, 0);
        pressingUser = collider.gameObject;

        gameManager.HandleCollision(gameObject);

        Debug.Log("Colliding with: " + collider.gameObject.name);
    }


}
