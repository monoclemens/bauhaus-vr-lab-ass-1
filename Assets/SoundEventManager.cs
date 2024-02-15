using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundEventManager : MonoBehaviour
{
    public static SoundEvent PlayPad = new();
    public static SoundSequenceEvent UpdateSoundSequence = new();
}

[System.Serializable]
public class SoundEvent : UnityEvent<string> { }

[System.Serializable]
public class SoundSequenceEvent : UnityEvent<SoundSequence> { }

// A class for serializing the sound sequence. Unity can't directly serialize an array, so this is necessary.
public class SoundSequence
{
    public string[] sequence;

    public SoundSequence(string[] sequence)
    {
        this.sequence = sequence;
    }
}
