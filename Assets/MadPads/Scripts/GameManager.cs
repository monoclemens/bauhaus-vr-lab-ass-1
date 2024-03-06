using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    /**
     * A dictionary that holds the name of a pad (MadPads_Pad.padName) as key 
     * and the pad itself as the value to play the needed pad fast.
     */
    private Dictionary<string, MadPads_Pad> padMap = new Dictionary<string, MadPads_Pad>();

    /** 
     * Initially set to Misty Mountains. 
     * Might make sense to turn it into a queue instead because in the current setup 
     * we are loading the sequence from reverse (see Start()).
     */
    private Stack<Tuple<string, double>> sequence = new Stack<Tuple<string, double>>();

    // This is our difficulty parameter so we need to add a slider to change this to make it less or more difficult.
    private double sequenceLength = 9.6;

    // These are the corresponding durations for note values (eigth -> 0.4 | quarter -> 0.8 | half -> 1.6 | dotted half -> 2.4).
    private List<double> possibleDurations = new List<double> { 0.4, 0.8, 1.6, 2.4 };

    private NetworkedAudioPlayer startButton;

    private bool firstStart = false;

    void Start()
    {
        GetPads();

        /**
         * Used to detect if the button is pressed.
         * Will need to change its name to, for example, StartButton.
         */
        startButton = GameObject.Find("InteractableCube").GetComponent<NetworkedAudioPlayer>();
        
        SetInitialSequence();

        // This is an event handler that catches collision detected in VirtualHand
        VirtualHand.OnCollision += HandleCollision;
    }

    #region OneTimeFunctions

    // Put all pads into the padMap upon start.
    private void GetPads()
    {
        Transform grids = GameObject.Find("MadPads").transform;

        // Add all pads into the pads list to then assign the audios.
        foreach (Transform padsGroup in grids)
        {
            Transform grid = padsGroup.GetChild(0);

            for (int index = 0; index < grid.childCount; index++)
            {
                Transform childPad = grid.GetChild(index);
                MadPads_Pad pad = childPad.GetChild(0).gameObject.GetComponent<MadPads_Pad>();

                if (pad != null)
                {
                    padMap.Add(pad.padName, pad);
                }
                else
                {
                    Debug.LogWarning($"childPad at index {index} is null.");
                }
            }
        }
    }

    // Misty Mountains
    private void SetInitialSequence()
    {
        sequence.Push(new Tuple<string, double>("Pad_TopLeftRightPads", 2.4));
        sequence.Push(new Tuple<string, double>("Pad_BottomRightRightPads", 0.8));
        sequence.Push(new Tuple<string, double>("Pad_BottomLeftRightPads", 0.8));
        sequence.Push(new Tuple<string, double>("Pad_CenterCenterRightPads", 0.4));
        sequence.Push(new Tuple<string, double>("Pad_TopCenterLeftPads", 0.4));
        sequence.Push(new Tuple<string, double>("Pad_CenterCenterRightPads", 0.8));
        sequence.Push(new Tuple<string, double>("Pad_BottomLeftRightPads", 0.8));
        sequence.Push(new Tuple<string, double>("Pad_TopLeftRightPads", 1.6));
        sequence.Push(new Tuple<string, double>("Pad_CenterCenterLeftPads", 0.8));
        sequence.Push(new Tuple<string, double>("Pad_TopLeftLeftPads", 0.8));
    }

    #endregion

    /**
     * All collisions land in this method, including pads and button(s).
     * If the collision is with a pad, the pad is played.
     * If it is with a button, the first hit triggers the initial sequence and initiates the pad audios.
     * Every hit after generates a new random sequence and plays it.
     * 
     * TODO: The initiation of audios need to take place in the second hitting of the button,
     *       so for now the changing of padaudio logic is faulty.
     */
    private void HandleCollision(GameObject collidedObject)
    {
        if(startButton.name == collidedObject.name)
        {
            if(!firstStart)
            {
                firstStart = true;

                Debug.Log("Game is Starting");

                // Access all pads and sync them up.
                foreach (var keyValuePair in padMap)
                {
                    MadPads_Pad pad = keyValuePair.Value;
                    pad.Sync();
                }
            }
            else
            {
                sequence = RandomSequenceGenerator();
            }

            SequencePlayer(sequence);
        }
        if (collidedObject.GetComponent<MadPads_Pad>() != null)
        {
            MadPads_Pad playedPad = collidedObject.GetComponent<MadPads_Pad>();
            playedPad.Play();
        }
    }

    #region StartButtonFunctions

    /**
     * Plays the given sequence popping all samples needing playing from the stack.
     */
    private void SequencePlayer(Stack<Tuple<string, double>> sequence)
    {
        double prevDuration = 0.0;

        while (sequence.Count > 0)
        {
            Tuple<string, double> sample = sequence.Pop();

            string padName = sample.Item1;
            double sampleDuration = sample.Item2;
            
            StartCoroutine(PlaySampleCoroutine(padName, sampleDuration, prevDuration));

            /**
             * !!!IMPORTANT!!!
             * This is not only the previous sample's playing duration but all
             * the time starting from the first iteration in this while loop.
             * Because a coroutine runs every frame.
             */
            prevDuration += sampleDuration;
        }
    }

    // A coroutine because playing the samples needs to wait for the previous one to finish.
    private IEnumerator PlaySampleCoroutine(string padName, double sampleDuration, double prevDuration)
    {
        // Check if the key exists in the padMap.
        if (padMap.ContainsKey(padName))
        {
            yield return new WaitForSeconds((float)prevDuration);

            padMap[padName].Play(sampleDuration);
        }
        else
        {
            Debug.LogError($"Sample with name '{padName}' not found in the padMap.");
        }
    }

    // Random sequence that adds up to sequenceLength is generated.
    private Stack<Tuple<string, double>> RandomSequenceGenerator()
    {
        Stack<Tuple<string, double>> randomSequence = new Stack<Tuple<string, double>>();

        List<string> padKeys = new List<string>(padMap.Keys);
        
        double remainingSum = sequenceLength;

        /**
         * Double precision causes problems here because sometimes there is 0.399999 time left for instance
         * 
         * TODO: Add epsilon tolerance.
         */
        while (remainingSum >= 0.4)
        {
            int randomSampleIndex = Random.Range(0, padMap.Count);

            string randomSample = padKeys[randomSampleIndex];

            double randomDuration = possibleDurations[Random.Range(0, possibleDurations.Count)];

            if (randomDuration <= remainingSum)
            {
                remainingSum -= randomDuration;
                randomSequence.Push(new Tuple<string, double>(randomSample, randomDuration));

                Debug.Log($"Sample with name '{randomSample}' with the duration: {randomDuration} with the remaining secs {remainingSum}");
            }
        }

        return randomSequence;
    }

    #endregion
}
