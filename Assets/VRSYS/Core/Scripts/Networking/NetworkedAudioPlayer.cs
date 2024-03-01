using Unity.Netcode;
using UnityEngine;
using VRSYS.Core.Logging;
using Unity.Collections;


public class NetworkedAudioPlayer : NetworkBehaviour
{
    #region Member Variables

    private string padName = "";
    //will need to implement this 
    private string side = "";

    public NetworkVariable<FixedString32Bytes> audioPath = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private AudioSource audioSource;

    private float clipLength;

    #endregion

    #region Initialization

    private void Awake()
    {
        padName = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject.name : "No Name";
        audioSource = GetComponent<AudioSource>();
        //networkedAudioSource.Value = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }


    }

    #endregion

    
    #region Audio Methods
    //only when choosing audio for pads no need to distribute
    public void LocallyPlayAudio()
    {
        ExtendedLogger.LogInfo(GetType().Name," Audio attached to " + padName + " on the " + side + " side changed!");
        audioSource.Play();
    }

    public void PlayAudio(double duration = 0)
    {
        /*if (isAudioPlaying.Value)
        {
            Debug.LogWarning("Audio is already playing.");
            return;
        }*/
        //the initial recognizable sequence
        if (duration == 0)
        {
            duration = clipLength;
        }


        //isAudioPlaying.Value = true;
        ExtendedLogger.LogInfo(GetType().Name, duration.ToString());
        PlayAudioServerRpc(duration);
    }

    public void SetAudio(string clipName = "")
    {
        FixedString32Bytes path = new FixedString32Bytes("audio/" + clipName);
        SetAudioServerRpc(path);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAudioServerRpc(FixedString32Bytes path)
    {
        audioPath.Value = path;
        ExtendedLogger.LogInfo(GetType().Name, audioPath.Value.ToString() + " burda degisir");
        SetAudioClientRpc();

    }

    
    [ClientRpc]
    private void SetAudioClientRpc()
    {
        ExtendedLogger.LogInfo(GetType().Name, audioPath.Value.ToString() + " clientdir");
        AudioClip tempAudioClip = Resources.Load<AudioClip>(audioPath.Value.ToString());
        if (tempAudioClip != null)
        {
            audioSource.clip = tempAudioClip;
            clipLength = tempAudioClip.length;
            //set it stereo i dunno what option is better
            audioSource.spatialBlend = 0f;
            ExtendedLogger.LogInfo(GetType().Name, "Successfully set audio!");
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
        ExtendedLogger.LogInfo(GetType().Name, duration.ToString());
        PlayAudioClientRpc(duration);
    }

    [ClientRpc]
    private void PlayAudioClientRpc(double duration)
    {
        if (audioSource.clip != null)
        {
            

            audioSource.Play();
            audioSource.SetScheduledEndTime(AudioSettings.dspTime + (duration));
            ExtendedLogger.LogInfo(GetType().Name, "PLAZED MAAAAN");
        }
        else
        {
            Debug.LogError("Audio clip not found. Make sure the file path is correct.");
        }
    }

    #endregion
}