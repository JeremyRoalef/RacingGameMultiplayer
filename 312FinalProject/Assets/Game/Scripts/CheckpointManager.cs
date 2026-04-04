using System;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [SerializeField]
    CheckpointGroup[] checkpointGroups;

    CheckpointGroup currentCheckpointGroup;
    Checkpoint currentCheckpoint;

    bool playerHasHitFirstCheckpoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple checkpoint managers found in scene");
            Destroy(gameObject);
        }

        if (checkpointGroups.Length == 0)
        {
            Debug.LogError("Error: Checkpoint group uninitialized for active scene");
        }

        foreach (CheckpointGroup checkpointGroup in checkpointGroups)
        {
            foreach (Checkpoint checkpoint in checkpointGroup.checkpoints)
            {
                checkpoint.OnPlayerTriggerEnter += HandlePlayerEnteredCheckpoint;
            }
        }

        SetCurrentCheckpointGroup(checkpointGroups.First());

        //Initialize player current checkpoint
        currentCheckpoint = checkpointGroups.First().checkpoints.First();
    }

    public void SetCurrentCheckpointGroup(CheckpointGroup checkpointGroup)
    {
        //Disable all current checkpoints
        if (currentCheckpointGroup != null)
        {
            foreach (Checkpoint checkpoint in currentCheckpointGroup.checkpoints)
            {
                checkpoint.gameObject.SetActive(false);
            }
        }

        //set new checkpoint group
        currentCheckpointGroup = checkpointGroup;

        //Enable all new checkpoints
        if (currentCheckpointGroup != null)
        {
            foreach (Checkpoint checkpoint in currentCheckpointGroup.checkpoints)
            {
                checkpoint.gameObject.SetActive(true);
            }
        }
    }

    void HandlePlayerEnteredCheckpoint(Checkpoint checkpoint, Vehicle vehicle)
    {
        currentCheckpoint = checkpoint;

        //Set new active checkpoints
        int currentCheckpointGroupIndex = Array.IndexOf(checkpointGroups, currentCheckpointGroup);
        int nextCheckpointGroupIndex = currentCheckpointGroupIndex + 1;

        if (nextCheckpointGroupIndex >= checkpointGroups.Length)
        {
            nextCheckpointGroupIndex = 0;
        }

        SetCurrentCheckpointGroup(checkpointGroups[nextCheckpointGroupIndex]);

        //If this was the first cheeckpoint, don't send to server.
        //This is to account for the first checkpoint accumulating laps.
        if (playerHasHitFirstCheckpoint)
        {
            //Handle player entered the checkpoint
            vehicle.UpdateCheckpointServerRpc(currentCheckpointGroupIndex);
        }
        else
        {
            playerHasHitFirstCheckpoint = true;
        }
    }

    public Checkpoint GetCurrentCheckpoint()
    {
        return currentCheckpoint;
    }
}
