using System.Collections;
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
    public string padName;

    public Color color;
    private bool isPlaying = false;

    NetworkedAudioPlayer audioPlayer;
    public GameManager gameManager;

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

        audioPlayer = GetComponent<NetworkedAudioPlayer>();
    }

    public void SyncColor(Color newColor)
    {
        color = newColor;

        InteractiveMaterial.SetColor(
            "_Color",
            color);

        TriangleMaterial.SetColor(
            "_EmissionColor",
            color);
    }

    /**
     * Function calling a networkedaudioplayer method.
     * 
     * TODO: Figure out if the playingID is necessary.
     */
    public void Play(double duration = 0, ulong playingID = 1000)
    {
        if (isPlaying) return;

        isPlaying = true;

        audioPlayer.PlayAudio(duration);

        // Trigger the validation from here because we don't want to trigger it for every collision between hand and pad.
        gameManager.ValidatePlayedSoundServerRpc(padName);

        var checkedDuration = duration == 0
            ? audioPlayer.clipLength
            : duration;

        ChangeInteractionColorServerRpc(checkedDuration);

        ResetInteractivityServerRpc(checkedDuration);
    }

    /**
     * This can be done locally, because the interaction color is always the same.
     * 
     * TODO: Make use of this when interacting.
     */
    private void TogglePadColor()
    {
        InteractiveMaterial.SetColor(
            "_Color",
            InteractiveMaterial.color == color
                ? Color.blue
                : color);
    }

    #region RPCs

    [ServerRpc(RequireOwnership = false)]
    public void ResetInteractivityServerRpc(double duration)
    {
        ResetInteractivityClientRpc(duration);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeInteractionColorServerRpc(double duration)
    {
        ChangeInteractionColorClientRpc(duration);
    }

    [ClientRpc]
    public void ChangeInteractionColorClientRpc(double duration)
    {
        TogglePadColor();

        StartCoroutine(ChangeInteractionColorCoroutine(duration));
    }

    [ClientRpc]
    public void ResetInteractivityClientRpc(double duration)
    {
        StartCoroutine(ResetInteractivityCoroutine(duration));
    }

    // Server checks if syncing is needed and forwards the updated path to clients.
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

    #region coroutines
    private IEnumerator ChangeInteractionColorCoroutine(double duration)
    {
        yield return new WaitForSeconds((float)duration);

        TogglePadColor();        
    }

    private IEnumerator ResetInteractivityCoroutine(double duration)
    {
        yield return new WaitForSeconds((float)duration);

        isPlaying = false;
    }

    #endregion
}
