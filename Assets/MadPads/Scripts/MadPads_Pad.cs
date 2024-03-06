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
    public string padName;

    // Distribute the triangle color, too, to show the color of the pad.
    public NetworkVariable<Color> color = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // This bool needs to be shared so all clients know if the a pad is being touched.
    private readonly NetworkVariable<bool> playing = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // A networked user ID to keep track of who touches a pad.
    private readonly NetworkVariable<ulong> touchingPlayer = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    NetworkedAudioPlayer audioPlayer;

    // Start is called before the first frame update
    void Start()
    {
        // Try to give the pad a name.
        if (gameObject.transform.parent != null)
        {
            padName = gameObject.transform
                .parent.gameObject.name;
        } 

        // Try to add another part to the end of its name?
        if (gameObject.transform.parent.transform.parent != null)
        {
            padName += gameObject.transform
                .parent.transform
                .parent.transform
                .parent.gameObject.name;
        }

        // If it doesn't have a name by now, give it a default name.
        if (padName == null)
        {
            padName = "No Name";
        }

        Debug.Log("Pad " + padName + " checking in!");

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

        audioPlayer = GetComponent<NetworkedAudioPlayer>();

    }

    // Update is called once per frame
    void Update()
    {

    }
    //function calling a networkedaudioplayer method
    public void Play(double duration = 0, ulong playingID = 1000)
    {
        audioPlayer.PlayAudio(duration);
        Debug.Log(duration.ToString() + padName);
    }

    // The listener changing the triangle color.
    public void OnColorChanged(Color previous, Color current)
    {
        TriangleMaterial.SetColor(
            "_EmissionColor",
            current);
    }
    // This can be done locally, because the interaction color is always the same.
    private void TogglePadColor()
    {
        InteractiveMaterial.color = InteractiveMaterial.color == _initialColor
            ? Color.blue
            : _initialColor;
    }
    #region RPCs

    //server checks if sync needed and forwards the updated path to clients
    public void Sync()
    {
        audioPlayer.SyncServerRpc();
    }
    #endregion
    #region Rendering
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
    #endregion
}
