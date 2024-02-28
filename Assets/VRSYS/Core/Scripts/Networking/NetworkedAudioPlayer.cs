using Unity.Netcode;
using UnityEngine;
using VRSYS.Core.Logging;

public class NetworkedAudioPlayer : NetworkBehaviour
{
    #region Member Variables

    [SerializeField]
    private AudioClip audioPath;

    private NetworkVariable<bool> isAudioPlaying = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private AudioSource audioSource;

    #endregion

    #region Initialization

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioPath = Resources.Load<AudioClip>("audio/initial_seq");
        audioSource.spatialBlend = 0f;
    }

    #endregion

    #region Audio Methods

    public void PlayAudio()
    {
        if (isAudioPlaying.Value)
        {
            Debug.LogWarning("Audio is already playing.");
            return;
        }

        isAudioPlaying.Value = true;
        PlayAudioServerRpc();
    }

    #endregion

    #region RPCs

    [ServerRpc(RequireOwnership = false)]
    private void PlayAudioServerRpc()
    {
        PlayAudioClientRpc();
    }

    [ClientRpc]
    private void PlayAudioClientRpc()
    {
        if (audioPath != null)
        {
            audioSource.clip = audioPath;
            ExtendedLogger.LogInfo(GetType().Name, "plazing");
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Audio clip not found. Make sure the file path is correct.");
        }
    }

    #endregion
}