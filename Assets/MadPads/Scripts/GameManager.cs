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

    // A map for keeping track of the color.
    private Dictionary<string, Color> padColorMap = new();

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

    private GameObject startButton;

    private bool isPlayingSequence = false;

    private NetworkVariable<int> startPressed = new(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<double> nextDuration = new(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    #region validation variables

    // The seconds determining how long they have to play the next correct pad.
    readonly float secondsToWait = 3f;
    Coroutine timeoutCoroutine;
    bool isTimeoutRunning = false;
    List<string> correctSequence = new();
    List<double> correctDurations = new();

    GameObject progressBar;
    public GameObject progressBarStepper;

    // A list of tuples to save the pad IDs, their steppers and their order.
    List<Tuple<string, GameObject>> progressBarSteppers = new();

    // The sequence that is currently being tracked in time.
    readonly List<string> currentlyTrackedSequence = new();

    // The current progress of the players. With this we can visualize the "progress bar".
    readonly List<string> correctlyPlayedSequence = new();

    #endregion

    void Start()
    {
        GetPads();

        /**
         * Used to detect if the button is pressed.
         * Will need to change its name to, for example, StartButton.
         */
        startButton = GameObject.Find("InteractableCube");
        
        SetInitialSequence();

        // This is an event handler that catches collision detected in VirtualHand
        VirtualHand.OnCollision += HandleCollision;
    }

    private void Update()
    {
        if(progressBar == null)
        {
            // Get a reference to the progress bar.
            progressBar = GameObject.Find("ProgressBar");
        }


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

                    // Attach this object to the pad.
                    pad.gameManager = this;
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

        GetListFromSequenceStack(sequence);
    }

    #endregion

    /**
     * Just a little helper to transform the stack of (padID | duration) tuples into a list of padIDs.
     */
    private void GetListFromSequenceStack(Stack<Tuple<string, double>> sequenceStack)
    {
        correctSequence.Clear();
        correctDurations.Clear();
        foreach (var pair in sequenceStack)
        {
            correctSequence.Add(pair.Item1);
            correctDurations.Add(pair.Item2);
        }
    }

   

    /**
     * All collisions land in this method, including pads and button(s).
     * If the collision is with a pad, the pad is played.
     * If it is with a button, the first hit triggers the initial sequence and initiates the pad audios.
     * Every hit after generates a new random sequence and plays it.
     * 
     * TODO: The initiation of audios need to take place in the second hitting of the button,
     *       so for now the changing of padaudio logic is faulty.
     */
    public void HandleCollision(GameObject collidedObject)
    {
        if (startButton.name == collidedObject.name && isPlayingSequence == false)
        {
            isPlayingSequence = true;
            StartHitServerRpc();
            StartCoroutine(ResetCubeInteractivity());
        }

        if (collidedObject.GetComponent<MadPads_Pad>() != null && startPressed.Value > 0)
        {
            MadPads_Pad playedPad = collidedObject.GetComponent<MadPads_Pad>();
            Debug.Log("Next duration is: " + nextDuration.Value.ToString());
            playedPad.Play(nextDuration.Value);
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

            PlaceStepperOnBarClientRpc(padName, prevDuration / sequenceLength);

            /**
             * !!!IMPORTANT!!!
             * This is not only the previous sample's playing duration but all
             * the time starting from the first iteration in this while loop.
             * Because a coroutine runs every frame.
             */
            prevDuration += sampleDuration;
        }
    }

    /**
     * This Coroutine only makes the cube interactable again after playing the sequence.
     */
    private IEnumerator ResetCubeInteractivity ()
    {
        yield return new WaitForSeconds((float)sequenceLength);

        isPlayingSequence = false;
    }

    // A coroutine because playing the samples needs to wait for the previous one to finish.
    private IEnumerator PlaySampleCoroutine(string padName, double sampleDuration, double prevDuration)
    {
        // Check if the key exists in the padMap.
        if (padMap.ContainsKey(padName))
        {
            yield return new WaitForSeconds((float)prevDuration);

            padMap[padName].Play(sampleDuration, true);
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

    /**
    * This RPC will create random colors for each pad and then tell every client to set that color.
    * This way the colors will be random but equal on all clients.
    */
    public void RandomlyChoosePadColor()
    {
        foreach (var keyValuePair in padMap)
        {
            Color randomColor = new(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));

            SetRandomlyChoosenPadColorClientRpc(keyValuePair.Key, randomColor);
        }
    }

    #endregion

    #region rpcs
    [ClientRpc]
    public void SetRandomlyChoosenPadColorClientRpc(string key, Color color)
    {
        padColorMap[key] = color;

        var pad = padMap[key];

        pad.SyncColor(color);
    }


    [ServerRpc(RequireOwnership = false)]
    public void ValidatePlayedSoundServerRpc(string padID)
    {
        Debug.Log($"Sound played by pad {padID}");

        Debug.Assert(correctSequence != null, "There is no defined sequence yet!");

        // Early return if there is no sequence.
        if (correctSequence == null) return;

        var currentIndex = currentlyTrackedSequence.Count;

        // Check if the played pad is the next correct one.
        if (padID == correctSequence[currentIndex])
        {
            Debug.Log("A correct pad was played!");

            ColorStepperClientRpc(currentIndex);

            if (isTimeoutRunning)
            {
                // Stop the countdown first.
                StopCoroutine(timeoutCoroutine);

                isTimeoutRunning = false;
            }

            // Add the pad's ID to their progress ...
            correctlyPlayedSequence.Add(padID);

            // ... and to the tracked IDs.
            currentlyTrackedSequence.Add(padID);

            // If the correctly played IDs are as long as the original sequence, they won.
            if (correctlyPlayedSequence.Count == correctSequence.Count)
            {
                Debug.Log("The players won!");

                // Now just tidy up.
                correctlyPlayedSequence.Clear();
                currentlyTrackedSequence.Clear();
            }
            else
            {
                //Keep track of the duration for the next play
                nextDuration.Value = correctDurations[currentIndex + 1];
                // Start it again if there is work left to do.
                timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
            }
        }
        else
        {
            Debug.LogWarning("A wrong pad was played.");

            // If the players make a mistake, clear the tracked sequence.
            currentlyTrackedSequence.Clear();
        }
    }

    [ClientRpc]
    public void ColorStepperClientRpc(int stepperIndex)
    {
        var stepperTuple = progressBarSteppers[stepperIndex];

        // Get the pad's color.
        var padColor = padColorMap[stepperTuple.Item1];

        // Get a reference to the stepper.
        var stepper = stepperTuple.Item2;

        // Get the Renderer component attached to the stepper.
        Renderer renderer = stepper.GetComponent<Renderer>();

        // Create a new material to prevent changing the color of all spheres sharing the same material.
        Material newMaterial = new(renderer.material)
        {
            color = padColor
        };

        // Assign the new material to the sphere.
        renderer.material = newMaterial;
    }
    [ServerRpc(RequireOwnership = false)]
    private void StartHitServerRpc()
    {
        startPressed.Value += 1;

        RandomlyChoosePadColor();

        if (startPressed.Value == 1)
        {
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

            // Keep track of the correct sequence, no matter what happens to the stack.
            GetListFromSequenceStack(sequence);

            // Reset the players' progress.
            currentlyTrackedSequence.Clear();
            correctlyPlayedSequence.Clear();

            ResetSteppersClientRpc();
        }
        //Keep track of the duration for the next play
        nextDuration.Value = correctDurations[0];

        SequencePlayer(sequence);
    }
    [ClientRpc]
    private void PlaceStepperOnBarClientRpc(string padID, double offsetPercentage)
    {
        if (!progressBar) return;
        //The bar length on y axis
        var barLength = progressBar.GetComponent<MeshFilter>().mesh.bounds.size.y;

        // Create a new stepper.
        var stepper = Instantiate(progressBarStepper);

        // Make it a child of the bar.
        stepper.transform.parent = progressBar.transform;

        var relativePositionOnBar = barLength * (float)offsetPercentage;
        var barRootOffset = -(barLength / 2);

        // Place it in the root of its parent, the bar, and apply the Y-offset relative to the bar's length.
        // Apply a negative offset, too, so the steppers don't start in the middle of the bar but in the left end of it.
        stepper.transform.localPosition = new(
            0,
            barRootOffset + relativePositionOnBar,
            0);
        // Set the initial scale
        stepper.transform.localScale = new Vector3(0.07f,1.4f,1.4f);

        // Set the initial local rotation
        stepper.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));

        var padStepperTuple = new Tuple<string, GameObject>(padID, stepper);
        progressBarSteppers.Add(padStepperTuple);
    }

    [ClientRpc]
    void ResetSteppersClientRpc()
    {
        foreach (var stepper in progressBarSteppers)
        {
            Destroy(stepper.Item2);
        }

        progressBarSteppers.Clear();
    }

    #endregion

    IEnumerator TimeoutCoroutine()
    {
        Debug.Log("Timeout started.");

        isTimeoutRunning = true;

        // Wait for a certain amount of time.
        yield return new WaitForSeconds(secondsToWait);

        Debug.Log("Timeout ran through.");

        isTimeoutRunning = false;

        // If the timeout has not been stopped, clear the tracked sequence.
        currentlyTrackedSequence.Clear();

        //Keep track of the duration for the next play
        nextDuration.Value = correctDurations[0];
    }
}
