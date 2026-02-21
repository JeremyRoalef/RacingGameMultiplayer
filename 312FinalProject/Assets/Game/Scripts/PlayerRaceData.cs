using UnityEngine;

public class PlayerRaceData
{
    public string PlayerName { get; set; }
    public int CurrentCheckpointIndex { get; set; }
    public int CompletedLaps { get; private set; }
    public float TimeSpentDuringRace { get; set; }
    public bool FinishedRace {  get; set; }

    public PlayerRaceData(
        string playerName, 
        int currentCheckpointIndex, 
        int currentLap, 
        float timeSpentDuringRace, 
        bool finishedRace
        )
    {
        this.PlayerName = playerName;
        this.CurrentCheckpointIndex = currentCheckpointIndex;
        this.CompletedLaps = currentLap;
        this.TimeSpentDuringRace = timeSpentDuringRace;
        this.FinishedRace = finishedRace;
    }

    public void SetCheckpointIndex(int newIndex)
    {
        if (newIndex == 0)
        {
            //player has lapped
            CompletedLaps++;
        }
    }
}
