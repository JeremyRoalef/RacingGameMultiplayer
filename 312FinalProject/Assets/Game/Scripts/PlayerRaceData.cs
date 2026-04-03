using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerRaceData : INetworkSerializable, IEquatable<PlayerRaceData>
{
    public ulong ClientID;
    public FixedString32Bytes PlayerName;
    public int CurrentCheckpointIndex;
    public int CompletedLaps;
    public float TimeSpentDuringRace;
    public bool FinishedRace;

    public static PlayerRaceData Invalid = new PlayerRaceData(99999, "", -1, -1, -1, false);

    public PlayerRaceData(
        ulong clientID, 
        FixedString32Bytes playerName, 
        int currentCheckpointIndex, 
        int completedLaps, 
        float timeSpentDuringRace, 
        bool finishedRace)
    {
        ClientID = clientID;
        PlayerName = playerName;
        CurrentCheckpointIndex = currentCheckpointIndex;
        CompletedLaps = completedLaps;
        TimeSpentDuringRace = timeSpentDuringRace;
        FinishedRace = finishedRace;
    }

    public bool Equals(PlayerRaceData other)
    {
        return other.ClientID == ClientID &&
            other.PlayerName == PlayerName &&
            other.CurrentCheckpointIndex == CurrentCheckpointIndex &&
            other.CompletedLaps == CompletedLaps &&
            other.TimeSpentDuringRace == TimeSpentDuringRace &&
            other.FinishedRace == FinishedRace;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref CompletedLaps);
        serializer.SerializeValue(ref CurrentCheckpointIndex);
        serializer.SerializeValue(ref CompletedLaps);
        serializer.SerializeValue(ref TimeSpentDuringRace);
        serializer.SerializeValue(ref FinishedRace);
    }
}
