using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAction : MonoBehaviour
{
    private Vector3 back;
    private Rigidbody rigidBody;
    private float originalValue;

    //Adjusts how powerful the push is
    private float forceConstant = 2f;

    //How much we want the "button" to go back
    private double buttonMargin = 0.2;

    void Start()
    {
        rigidBody = this.GetComponent<Rigidbody>();
        

        //the case of start button
        if(gameObject.name == "InteractableCube")
        {
            back = Vector3.forward;
            originalValue = this.transform.localPosition.z;
        }
        //The case of pads
        else
        {
            string greatGrandParentName = getGreatGrandParentName();
            if (greatGrandParentName == "LeftPads")
            {
                back = Vector3.left;
                originalValue = -(this.transform.localPosition.x);
            }
            else
            {
                back = Vector3.right;
                originalValue = -(this.transform.localPosition.x);
            }
        }

        VirtualHand.OnCollision += backwardsMotion;
    }

    // Update is called once per frame
    void Update()
    {
        float axisValue = Vector3.Dot(back, transform.localPosition);

        //Revert the motion if the threshold is exceeded
        if (axisValue >= originalValue + buttonMargin)
        {
            forwardMotion();
        }
        //Make the object stop if the button is back in the original location
        if (axisValue <= originalValue)
        {
            rigidBody.velocity = Vector3.zero;
        }


    }

    private void backwardsMotion(GameObject collidedObject)
    {
        if(rigidBody.velocity == Vector3.zero && collidedObject == this.gameObject)
        {
            rigidBody.AddForce(back * forceConstant, ForceMode.Impulse);
        }
    }
    private void forwardMotion()
    {
        rigidBody.AddForce(-back * forceConstant, ForceMode.Impulse);
    }

    private string getGreatGrandParentName()
    {

        return gameObject.transform
                .parent.transform
                .parent.transform
                .parent.gameObject.name;
    }

}
