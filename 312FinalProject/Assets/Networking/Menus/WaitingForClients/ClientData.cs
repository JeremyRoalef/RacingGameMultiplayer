using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct ClientData : INetworkSerializable, IEquatable<ClientData>
{
    public ulong ClientID;
    public FixedString32Bytes PlayerName;

    public ClientData(
        ulong clientID,
        FixedString32Bytes playerName
        )
    {
        ClientID = clientID;
        PlayerName = playerName;
    }

    public bool Equals(ClientData other)
    {
        return other.ClientID == ClientID &&
            other.PlayerName == PlayerName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref PlayerName);
    }

    public override string ToString()
    {
        return ClientID.ToString() + " " + PlayerName.ToString();
    }
}
