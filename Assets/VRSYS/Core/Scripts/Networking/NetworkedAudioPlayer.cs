using Unity.Netcode;
using UnityEngine;
using VRSYS.Core.Logging;

public class NetworkedAudioPlayer : NetworkBehaviour
{
    #region Member Variables

    [SerializeField]
    private AudioClip audioClip;

    private NetworkVariable<bool> isAudioPlaying = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private AudioSource audioSource;

    private float clipLength;

    #endregion

    #region Initialization

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioClip = Resources.Load<AudioClip>("audio/initial_seq");
        clipLength = audioClip.length;
        audioSource.spatialBlend = 0f;
    }

    #endregion

    #region Audio Methods

    public void PlayAudio(float duration = 0)
    {
        //this shouldn't be here when we go into pad playing
        if (isAudioPlaying.Value)
        {
            Debug.LogWarning("Audio is already playing.");
            return;
        }
        if (duration == 0)
        {
            duration = clipLength;
        }

        isAudioPlaying.Value = true;
        PlayAudioServerRpc(duration);
    }

    #endregion

    #region RPCs

    [ServerRpc(RequireOwnership = false)]
    private void PlayAudioServerRpc(float duration)
    {
        PlayAudioClientRpc(duration);
    }

    [ClientRpc]
    private void PlayAudioClientRpc(float duration)
    {
        if (audioClip != null)
        {
            
            audioSource.clip = audioClip;
            //ExtendedLogger.LogInfo(GetType().Name, duration.ToString());
            audioSource.Play();
            audioSource.SetScheduledEndTime(AudioSettings.dspTime + (duration));
        }
        else
        {
            Debug.LogError("Audio clip not found. Make sure the file path is correct.");
        }
    }

    #endregion
}