using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @Cem
 * 
 * This way you should be able to fire events:
 * 
    string[] idSequence = { "1", "2" };

    SoundSequence newSequence = new(idSequence);

    SoundEventManager.UpdateSoundSequence.Invoke(newSequence);
 */
public class Validator : MonoBehaviour
{
    readonly float secondsToWait = 3f;

    Coroutine timeoutCoroutine;
    bool isTimeoutRunning = false;

    // The sequence they're supposed to play.
    string[] sequence;

    // The sequence that is currently being tracked in time.
    readonly List<string> currentlyTrackedSequence = new();

    // The current progress of the players. With this we can visualize the "progress bar".
    readonly List<string> correctlyPlayedSequence = new();

    void OnEnable()
    {
        // Subscribe to the events.
        SoundEventManager.PlayPad.AddListener(OnPadPlayed);
        SoundEventManager.UpdateSoundSequence.AddListener(OnSequenceUpdated);
    }

    void OnDisable()
    {
        // Unsubscribe when the GameObject is disabled or destroyed.
        SoundEventManager.PlayPad.RemoveListener(OnPadPlayed);
        SoundEventManager.UpdateSoundSequence.RemoveListener(OnSequenceUpdated);
    }

    void OnPadPlayed(string padID)
    {
        Debug.Log($"Sound played by pad {padID}");

        Debug.Assert(sequence != null, "There is no defined sequence yet!");

        // Early return if there is no sequence.
        if (sequence == null) return;

        // Check if the played pad is the next correct one.
        if (padID == sequence[currentlyTrackedSequence.Count])
        {
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
            if (correctlyPlayedSequence.Count == sequence.Length)
            {
                Debug.Log("The players won!");

                // Now just tidy up.
                correctlyPlayedSequence.Clear();
                currentlyTrackedSequence.Clear();
            } else
            { 
                // Start it again if there is work left to do.
                timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
            }
        } else
        {
            // If the players make a mistake, clear the tracked sequence.
            currentlyTrackedSequence.Clear();
        }
    }

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
    }

    void OnSequenceUpdated(SoundSequence soundSequence)
    {
        sequence = soundSequence.sequence;

        currentlyTrackedSequence.Clear();

        correctlyPlayedSequence.Clear();
    }
}