using Unity.Netcode;
using UnityEngine;
using VRSYS.Core.Logging;
using Unity.Collections;
using UnityEngine.InputSystem;



public class NetworkedAudioPlayer : NetworkBehaviour
{
    #region Member Variables


    public NetworkVariable<FixedString32Bytes> audioPath = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isPlaying = new NetworkVariable<bool>(false , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool syncNeeded = false;

    private AudioSource audioSource;
    private float clipLength;
    private FixedString32Bytes uiAudioPath;
    //for those pads whose samples were changed before the start of the server
    


    #endregion

    #region Initialization

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 0f;

    }

    private void Update()
    {
        audioPath.OnValueChanged += (prev, curr) =>
        {
            if(syncNeeded)
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


        //a server RPC is needed for all the clients to be able to call this and alert the server
        //which will in turn make all the clients call the PlayAudioClientRpc
        PlayAudioServerRpc(duration);
    }

    public void SetAudio(string clipName = "")
    {
        FixedString32Bytes path = new FixedString32Bytes("audio/" + clipName);
        if(clipName.StartsWith("samples/"))
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

    //only when choosing audio for pads cannot distribute because the server is not yet initiated
    //so we need to do it in sync with the start button
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
        if (audioSource.clip != null)
        {
            audioSource.Play();
            audioSource.SetScheduledEndTime(AudioSettings.dspTime + (duration));
            ExtendedLogger.LogInfo(GetType().Name, duration.ToString());
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
        ExtendedLogger.LogInfo(GetType().Name, "did it change? " + path.ToString());
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
        if(syncNeeded)
        {
            audioPath.Value = uiAudioPath;
            ExtendedLogger.LogInfo(GetType().Name, "Audio synced to " + audioPath.Value.ToString());
        }
        
    }


    #endregion
}