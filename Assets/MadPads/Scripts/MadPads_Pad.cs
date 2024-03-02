using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/**
 * Hi Cem!
 * 
 * I'm not sure if we need to distribute the sound, too. If the sounds are set in the lobby,
 * every client should have the sound upon Start. If not, we might need to distribute it.
 * 
 * Besides, I'm not sure if the pad is automatically the spatial source of the sound. 
 * It might, because I set the Spatial Blend of the Pad Prefab to 1.
 * 
 * Anyway, important network variables for you are "sound" and "color".
 */

public class MadPads_Pad : NetworkBehaviour
{
    
    public UnityEvent onTouch;
    public UnityEvent onLeave;
    private Color _initialColor;
    private string padName;



    // Distribute the triangle color, too, to show the color of the pad.
    public NetworkVariable<Color> color = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // This bool needs to be shared so all clients know if the a pad is being touched.
    private readonly NetworkVariable<bool> isTouched = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // A networked variable to keep track of who touches a pad.
    //changed it to ulong to have the client id serialized not the gameobject 
    private readonly NetworkVariable<ulong> touchingPlayer = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // A singleton for the renderer with a corresponding getter.
    private Renderer _renderer;
    public Renderer Renderer
    {
        get
        {
            if (_renderer == null)
            {
                _renderer = gameObject.GetComponent<Renderer>();
            }

            Debug.Assert(_renderer != null, "Could not find Renderer of pad " + gameObject.name + "!");

            return _renderer;
        }
    }

    // Another singleton for the interactive material.
    private Material _interactiveMaterial;
    public Material InteractiveMaterial
    {
        get
        {
            if (_interactiveMaterial == null)
            {
                // The element at index 1 is the interactive material.
                _interactiveMaterial = Renderer.materials[1];
            }

            Debug.Assert(_interactiveMaterial != null, "Could not find interactive material of pad " + gameObject.name + "!");

            return _interactiveMaterial;
        }
    }

    // Another singleton for the triangle material.
    private Material _triangleMaterial;
    public Material TriangleMaterial
    {
        get
        {
            if (_triangleMaterial == null)
            {
                // The element at index 2 is the triangle material.
                _triangleMaterial = Renderer.materials[2];
            }

            Debug.Assert(_triangleMaterial != null, "Could not find triangle material of pad " + gameObject.name + "!");

            return _triangleMaterial;
        }
    }

    // This can be done locally, because the interaction color is always the same.
    private void TogglePadColor()
    {
        InteractiveMaterial.color = InteractiveMaterial.color == _initialColor
            ? Color.blue
            : _initialColor;
    }

    // Start is called before the first frame update
    void Start()
    {
        padName = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject.name : "No Name";
        //uglyyy i know
        padName += gameObject.transform.parent.transform.parent != null ? gameObject.transform.parent.transform.parent.transform.parent.gameObject.name : "No Name";
        Debug.Log("Pad " + padName + " checking in!");

        // Here we distribute a bool, this might be unnecessary and we could probably just initialite it with false.
        isTouched.Value = false;

        // Locally keep track of the initial color. No need to distribute.
        _initialColor = InteractiveMaterial.GetColor("_Color");

        // The following code just demonstrates how to change the color of materials, @Cem.
        Color testColor = new(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));

        InteractiveMaterial.SetColor(
            "_Color",
            testColor);

        TriangleMaterial.SetColor(
            "_EmissionColor",
            testColor);

        /**
         * Attach a listener to the color network variable.
         * TODO: We might need to "subtract" it on destroy. Check!
         */
        color.OnValueChanged += OnColorChanged;


    }

    // The listener changing the triangle color.
    public void OnColorChanged (Color previous, Color current)
    {
        TriangleMaterial.SetColor(
            "_EmissionColor",
            current);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*// This method will be executed when a player collides with the collider of the pad.
    private void OnTriggerEnter(Collider playerCollider)
    {
        // Early return if it is already being touched.
        if (isTouched.Value) return;

        // Let everyone know who touches the pad.
        touchingPlayer.Value = playerCollider.gameObject;

        // Set the bool now so all clients learn about the change.
        isTouched.Value = true;

        // And trigger the sound, color etc. on every client.
        PlayOnAllClientsClientRpc();
    }

    [ClientRpc]
    void PlayOnAllClientsClientRpc()
    {
        // Locally change the pads color for everyone.
        TogglePadColor();

        // TODO: Check if this needs to happen: https://www.youtube.com/watch?v=lPPa9y_czlE
        onTouch.Invoke();

        // If there is a sound, play it now.
        if (sound.Value != null)
        {
            sound.Value.Play();
        }
    }

    // This method will be executed when a player collides with the collider of the pad.
    private void OnTriggerExit(Collider playerCollider)
    {
        // Early return if the one releasing is not the one currently touching it.
        if (playerCollider.gameObject != touchingPlayer.Value) return;

        // Let everyone know that the pad is not being touched anymore.
        isTouched.Value = false;

        StopOnAllClientsClientRpc();
    }

    [ClientRpc]
    void StopOnAllClientsClientRpc()
    {
        // Locally change the pads color for everyone.
        TogglePadColor();

        // TODO: Check if this needs to happen: https://www.youtube.com/watch?v=lPPa9y_czlE
        onLeave.Invoke();

        // If there is a sound playing, stop it now.
        if (sound.Value != null
            && sound.Value.isPlaying)
        {
            sound.Value.Stop();
        }
    }*/
}
