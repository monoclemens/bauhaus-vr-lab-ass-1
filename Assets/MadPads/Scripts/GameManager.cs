using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VirtualHand;
public class GameManager : MonoBehaviour
{
    public List<NetworkedAudioPlayer> padAudioPlayers;
    private NetworkedAudioPlayer startButton;
    void Start()
    {
        startButton = GameObject.Find("InteractableCube").GetComponent<NetworkedAudioPlayer>();
        VirtualHand.OnCollision += HandleCollision;
    }

    void Update()
    {
    }

    private void StartGame(GameObject objectToPlay)
    {
        startButton.PlayAudio();
    }

    public void getPads(List<NetworkedAudioPlayer> audioPlayers)
    {
        padAudioPlayers = audioPlayers;
    }
    private void HandleCollision(GameObject collidedObject)
    {
        if(startButton.name == collidedObject.name)
        {
            Debug.Log("Game is Starting");
            foreach(NetworkedAudioPlayer pad in padAudioPlayers)
            {
                pad.gameObject.GetComponent<MadPads_Pad>().Sync();
            }
            
        }
        
    }
}
