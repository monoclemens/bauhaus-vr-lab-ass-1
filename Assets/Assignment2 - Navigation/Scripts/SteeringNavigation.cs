using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringNavigation : MonoBehaviour
{
    public InputActionProperty steeringAction;

    public Transform navigationOrigin;
    public Transform steeringHand;

    // TODO: What is this variable for? We don't need it for the Update.
    public float moveSpeed = 2f;
    private float moveThreshhold = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        if (navigationOrigin == null)
        {
            navigationOrigin = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float steeringInput = steeringAction.action.ReadValue<float>();

        /************************************
         * Exercise 2.6 Steering Navigation *
         ************************************/

        // If the input is not greater than the threshold, just stop the update.
        if (steeringInput <= moveThreshhold)
        {
            return;
        }

        // Get the delta speed so the movement starts smoothly.
        float deltaSpeed = steeringInput - moveThreshhold;

        transform.position +=
            // Use the speed a.k.a the force on the steering hand's controller's input.
            deltaSpeed
            // Include the time since the last frame so the movement is smooth.
            * Time.deltaTime
            // Move into the forward direction of the steering hand.
            * steeringHand.forward;
    }
}
