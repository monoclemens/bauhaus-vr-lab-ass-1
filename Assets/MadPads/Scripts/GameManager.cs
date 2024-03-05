using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using static VirtualHand;
public class GameManager : NetworkBehaviour
{
    private Dictionary<string, MadPads_Pad> padMap = new Dictionary<string, MadPads_Pad>();
    private Stack<Tuple<string, double>> sequence = new Stack<Tuple<string, double>>();
    private double sequenceLength;


    private NetworkedAudioPlayer startButton;

    private bool firstStart = false;
    void Start()
    {
        getPads();
        startButton = GameObject.Find("InteractableCube").GetComponent<NetworkedAudioPlayer>();
        sequence.Push(new Tuple<string, double>("Pad_TopLeftLeftPads", 1.0));
        sequence.Push(new Tuple<string, double>("Pad_TopLeftRightPads", 2.0));
        sequenceLength = 9.6;
        VirtualHand.OnCollision += HandleCollision;
    }

    void Update()
    {
    }

    private void StartGame(GameObject objectToPlay)
    {
        startButton.PlayAudio();
    }

    private void getPads()
    {

        Transform grids = GameObject.Find("MadPads").transform;
        //add all pads into the pads list to then assign the audios
        foreach (Transform padsGroup in grids)
        {
            Transform grid = padsGroup.GetChild(0);
            for (int i = 0; i < grid.childCount; i++)
            {
                // Access the i-th child
                Transform childPad = grid.GetChild(i);
                MadPads_Pad pad = childPad.GetChild(0).gameObject.GetComponent<MadPads_Pad>();
                if (pad != null)
                {
                    padMap.Add(pad.padName, pad);
                }
                else
                {
                    Debug.LogWarning("childPad is null");
                }
            }


        }
    }
    private void HandleCollision(GameObject collidedObject)
    {
        if(startButton.name == collidedObject.name)
        {
            //first start should be the initial sequence
            //starts after the first should be new Random sequence generation
            //play the random sequence
            if(!firstStart)
            {
                firstStart = true;
                Debug.Log("Game is Starting");
                foreach (var kvp in padMap)
                {
                    MadPads_Pad pad = kvp.Value;
                    pad.Sync();
                }
            }
            else
            {
                sequence = RandomSequenceGenerator();
            }
            SequencePlayer(sequence);
        }
        
    }
    private void SequencePlayer(Stack<Tuple<string, double>> sequence)
    {
        double prevDuration = 0.0;
        string sampleName = "";
        double sampleDuration = 0;
        while (sequence.Count > 0)
        {
            Tuple<string, double> sample = sequence.Pop();

            sampleName = sample.Item1;
            sampleDuration = sample.Item2;

            
            StartCoroutine(PlaySampleCoroutine(sampleName, sampleDuration, prevDuration));
            prevDuration = sampleDuration;
        }

    }

    // Example coroutine method
    private IEnumerator PlaySampleCoroutine(string sampleName, double sampleDuration, double prevDuration)
    {
        // Check if the key exists in the dictionary
        if (padMap.ContainsKey(sampleName))
        {
            yield return new WaitForSeconds((float)prevDuration); // Wait for 1 second
            padMap[sampleName].Play(sampleDuration);
        }
        else
        {
            Debug.LogError($"Sample with name '{sampleName}' not found in the dictionary.");
        }
    }
    private Stack<Tuple<string, double>> RandomSequenceGenerator()
    {
        Stack<Tuple<string, double>> randomSequence = new Stack<Tuple<string, double>>();


        return randomSequence;

    }
}
