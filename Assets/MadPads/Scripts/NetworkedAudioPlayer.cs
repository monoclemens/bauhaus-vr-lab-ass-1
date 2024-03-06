using Unity.Netcode;
using UnityEngine;
using VRSYS.Core.Logging;
using Unity.Collections;
using System.Collections;

public class NetworkedAudioPlayer : NetworkBehaviour
{
    #region Member Variables

    public NetworkVariable<FixedString32Bytes> audioPath = new(
        new FixedString32Bytes(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<bool> isPlaying = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool syncNeeded = false;
    private AudioSource audioSource;
    private float clipLength;

    // For those pads whose samples were changed before the start of the server.
    private FixedString32Bytes uiAudioPath;

    #endregion

    #region Initialization

    /**
     * Upon awakening, attach an audio source if it's missing and make the sound 2D.
     */
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // This removes any 3D influence on the sound.
        audioSource.spatialBlend = 0f;
    }

    /**
     * This attaches a listener to the audio player for every frame.
     * The listener sets the audio source of the pad if a sync is necessary,
     * i.e. a new "sample dropped" for the pad.
     * 
     * TODO: Check if it's necessary to attach the listener in every frame.
     */
    private void Update()
    {
        audioPath.OnValueChanged += (prev, curr) =>
        {
            if (syncNeeded)
            {
                syncNeeded = false;
                SetAudioClientRpc(curr);
            }
        };
    }

    #endregion

    #region Audio Methods

    private bool sourcePlaying
    {
        get
        {
            return audioSource.isPlaying;
        }
    }

    public void PlayAudio(double duration = 0)
    {
        if (duration == 0)
        {
            duration = clipLength;
        }

        /**
         * A server RPC is needed for all the clients to be able to call this and alert the server.
         * Which will in turn make all the clients call the PlayAudioClientRpc.
         */
        PlayAudioServerRpc(duration);
    }

    /**
     * This method sets the audio source of the pad.
     * It expects the path/name of the source.
     * If the source is one of the default samples, it will locally set it for each client.
     * Otherwise it will call a server RPC to do that in a distributed manner.
     * 
     * TODO: Check if the server RPC works already.
     */
    public void SetAudio(string clipName = "")
    {
        FixedString32Bytes path = new("audio/" + clipName);

        if (clipName.StartsWith("samples/"))
        {
            syncNeeded = true;
            LocallySetAudio(path);
        }
        else
        {
            SetAudioServerRpc(path);
            ExtendedLogger.LogInfo(GetType().Name, path.ToString());
        }
    }

    /**
     * When choosing audio source for the pads in the lobby, they can't be distributed yet, because the server is not ready yet.
     * So we need to do it in sync with the start button.
     * 
     * TODO: Add logic to the start button so upon first collision, it checks for existing audio sources 
     *       and adds them to the pads.
     */
    public void LocallyPlayAudio()
    {
        audioSource.Play();
    }

    public void LocallySetAudio(FixedString32Bytes path)
    {

        AudioClip tempAudioClip = Resources.Load<AudioClip>(path.ToString());

        if (tempAudioClip != null)
        {
            uiAudioPath = path;
        }
        else
        {
            Debug.LogWarning("No such audio");
        }
    }

    /**
     * This method ensures the audio fades out smoothly instead of being cut off 
     * like a Bauhaus student's long hair.
     */
    private IEnumerator FadeOut(float duration, float fadeTime = 0.1f)
    {
        float startVolume = audioSource.volume;
        float waitTime = duration - fadeTime;

        yield return new WaitForSeconds(waitTime);

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    #endregion

    #region RPCs

    [ServerRpc(RequireOwnership = false)]
    private void PlayAudioServerRpc(double duration)
    {
        PlayAudioClientRpc(duration);
    }

    [ClientRpc]
    private void PlayAudioClientRpc(double duration)
    {
        if (audioSource.clip != null)
        {
            StartCoroutine(FadeOut((float)duration));
            audioSource.Play();
            audioSource.SetScheduledEndTime(AudioSettings.dspTime + (duration));
        }
        else
        {
            Debug.LogError("Audio clip not found. Make sure the file path is correct.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAudioServerRpc(FixedString32Bytes path)
    {
        ExtendedLogger.LogInfo(GetType().Name, path.ToString());

        audioPath.Value = path;
    }

    [ClientRpc]
    public void SetAudioClientRpc(FixedString32Bytes path)
    {
        AudioClip tempAudioClip = Resources.Load<AudioClip>(path.ToString());
        if (tempAudioClip != null)
        {
            audioSource.clip = tempAudioClip;
            clipLength = tempAudioClip.length;
            ExtendedLogger.LogInfo(GetType().Name, "Successfully set audio and distributed!");
        }
        else
        {
            Debug.LogWarning("No such audio");
            return;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncServerRpc()
    {
        if (syncNeeded)
        {
            audioPath.Value = uiAudioPath;
            ExtendedLogger.LogInfo(GetType().Name, "Audio synced to " + audioPath.Value.ToString());
        }

    }

    #endregion
}