using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HandCollider : MonoBehaviour
{
    #region Member Variables

    public bool isColliding { get; private set; } = false;
    public GameObject collidingObject { get; private set; }

    #endregion

    #region MonoBehaviour Callbacks

    /**
     * If a collision is detected and there's currently no trace of a collision, 
     * this method will store the information about the new collision.
     * 
     * What if there's another collision while we already stored an older collision?
     * This code means there can only ever be one collision at a time, any other will be disregarded.
     */
    private void OnTriggerEnter(Collider other)
    {
        if (!isColliding)
        {
            isColliding = true;
            collidingObject = other.gameObject;
        }
    }

    /**
     * If the currently stored collider "exits" the collision, we'll reset the stored data.
     */
    private void OnTriggerExit(Collider other)
    {
        if (isColliding && other.gameObject == collidingObject)
        {
            collidingObject = null;
            isColliding = false;
        }
    }

    #endregion
}
