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
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 0f;

    }

    #endregion

    
    #region Audio Methods
    

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
        if(clipName.StartsWith("samples/"))
        {
            LocallySetAudio(path);
        }
        else
        {
            
            SetAudioServerRpc(path);
            ExtendedLogger.LogInfo(GetType().Name, path.ToString());
        }
    }

    //only when choosing audio for pads no need to distribute
    public void LocallyPlayAudio()
    {
        ExtendedLogger.LogInfo(GetType().Name," Audio attached to " + padName + " on the " + side + " side changed!");
        audioSource.Play();
    }
    
    public void LocallySetAudio(FixedString32Bytes path)
    {
        audioPath.Value = path;
        AudioClip tempAudioClip = Resources.Load<AudioClip>(audioPath.Value.ToString());
        if (tempAudioClip != null)
        {
            audioSource.clip = tempAudioClip;
            clipLength = tempAudioClip.length;            
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
    //not needed!!! just bypass it and go from the local play to client
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
    [ServerRpc(RequireOwnership = false)]
    private void SetAudioServerRpc(FixedString32Bytes path)
    {
        ExtendedLogger.LogInfo(GetType().Name, path.ToString());
        audioPath.Value = path;
        SetAudioClientRpc();

    }

    
    [ClientRpc]
    private void SetAudioClientRpc()
    {
        AudioClip tempAudioClip = Resources.Load<AudioClip>(audioPath.Value.ToString());
        if (tempAudioClip != null)
        {
            audioSource.clip = tempAudioClip;
            clipLength = tempAudioClip.length;            
            ExtendedLogger.LogInfo(GetType().Name, "Successfully set audio!");
        }
        else
        {
            Debug.LogWarning("No such audio");
            return;
        }
    }

    #endregion
}