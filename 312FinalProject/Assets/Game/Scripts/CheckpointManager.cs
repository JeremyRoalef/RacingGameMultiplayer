using System;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField]
    CheckpointGroup[] checkpointGroups;

    CheckpointGroup currentCheckpointGroup;
    Checkpoint currentCheckpoint;

    private void Awake()
    {
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

        //Handle player entered the checkpoint
        int currentCheckpointGroupIndex = Array.IndexOf(checkpointGroups, currentCheckpointGroup);
        vehicle.UpdateCheckpointServerRpc(currentCheckpointGroupIndex);

        //Set new active checkpoints
        int nextCheckpointGroupIndex = currentCheckpointGroupIndex + 1;

        if (nextCheckpointGroupIndex >= checkpointGroups.Length)
        {
            nextCheckpointGroupIndex = 0;
        }

        SetCurrentCheckpointGroup(checkpointGroups[nextCheckpointGroupIndex]);
    }
}
