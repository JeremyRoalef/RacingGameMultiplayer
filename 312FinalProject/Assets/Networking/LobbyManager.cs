using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour, IInitializable
{
    public static LobbyManager instance;
    public NetworkList<ulong> Clients = new NetworkList<ulong>();
    Dictionary<ulong, FixedString32Bytes> clientNames;
    bool initializedOnStart = false;
    bool initializedOnNetworkSpawn = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (clientNames == null)
        {
            clientNames = new Dictionary<ulong, FixedString32Bytes>();
        }
    }

    private void Start()
    {
        if (IsClient)
        {
            //Get the player's name from the network session
            string playerName = NetworkSession.instance.PlayerName;

            //Send the player's name to the server
            SendPlayerNameRpc(OwnerClientId, (FixedString32Bytes)playerName);
        }

        initializedOnStart = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Debug.Log("Spawned over network");

        if (!IsServer)
        {
            initializedOnNetworkSpawn = true;
            return;
        }
        //Manually add host
        Clients.Add(NetworkManager.LocalClientId);

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

        initializedOnNetworkSpawn = true;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        base.OnNetworkDespawn();
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;

        //Send host name to the server
        string playerName = NetworkSession.instance.PlayerName;
        SendPlayerNameRpc(NetworkManager.LocalClientId, (FixedString32Bytes)playerName);
    }

    private void HandleClientDisconnected(ulong obj)
    {
        if (!IsServer) return;
        Clients.Remove(obj);
    }

    private void HandleClientConnected(ulong obj)
    {
        if (!IsServer) return;
        Clients.Add(obj);
    }

    public void RequestToKickPlayer(ulong clientID)
    {
        if (!IsServer) return;

        KickPlayerServerRpc(clientID);
    }

    [Rpc(SendTo.Server)]
    private void KickPlayerServerRpc(ulong clientID)
    {
        Debug.Log("kicking player");

        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientID))
        {
            NetworkManager.Singleton.DisconnectClient(clientID);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    void SendPlayerNameRpc(ulong clientID, FixedString32Bytes playerName)
    {
        clientNames[clientID] = playerName;
        SendNewPlayerNameRpc(clientID, playerName);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SendNewPlayerNameRpc(ulong clientID, FixedString32Bytes playerName)
    {
        if (clientNames == null) clientNames = new Dictionary<ulong, FixedString32Bytes>();
        clientNames[clientID] = playerName;
    }

    public string GetClientName(ulong clientID)
    {
        if (!IsServer)
        {
            Debug.Log("clients don't have access to this dictionary");
            return "";
        }

        if (clientNames.ContainsKey(clientID))
        {
            return clientNames[clientID].ToString();
        }
        else
        {
            return "";
        }
    }

    public bool IsInitialized()
    {
        return initializedOnStart && initializedOnNetworkSpawn;
    }
}
