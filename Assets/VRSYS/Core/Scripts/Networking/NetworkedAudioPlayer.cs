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
        
    }

    #endregion

    #region Update
    /*void Update()
    {
        Debug.Log(isAudioPlaying.Value.ToString());
        if (isAudioPlaying.Value)
        {
            if (!audioSource.isPlaying)
            {
                // The audio clip has finished playing
                Debug.Log("Audio clip has finished playing.");

                // Set isAudioPlaying to false when the audio finishes playing
                isAudioPlaying.Value = false;

                // You can perform any actions or logic here after the audio clip has finished playing
            }
        }
    }*/
    #endregion
    #region Audio Methods

    public void PlayAudio( double duration = 0)
    {
        if (isAudioPlaying.Value)
        {
            Debug.LogWarning("Audio is already playing.");
            return;
        }
        //the initial recognizable sequence
        if (duration == 0)
        {
            duration = clipLength;
        }
       

        isAudioPlaying.Value = true;
        PlayAudioServerRpc(duration);
    }
    public void setAudio(string audioPath = "")
    {
        AudioClip tempAudioClip = Resources.Load<AudioClip>(audioPath);
        if (tempAudioClip != null)
        {
            audioClip = tempAudioClip;
            clipLength = audioClip.length;
            //set it stereo i dunno what option is better
            audioSource.spatialBlend = 0f;
        }
        else
        {
            Debug.LogWarning("No such audio");
            return;
        }
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