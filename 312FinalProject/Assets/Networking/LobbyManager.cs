using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour, IInitializable
{
    [SerializeField]
    NetworkObject lobbyManagerPrefab;

    public static LobbyManager Instance;
    public NetworkList<ClientData> clientData = new NetworkList<ClientData>();
    bool initializedOnStart = false;
    bool initializedOnNetworkSpawn = false;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        initializedOnStart = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient && !IsServer)
        {
            SendPlayerNameRpc(NetworkSession.instance.PlayerName);
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;


            //Manually add host
            ulong hostId = NetworkManager.LocalClientId;

            clientData.Add(new ClientData(
                hostId,
                NetworkSession.instance.PlayerName
            ));
        }

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
    }

    private void HandleClientDisconnected(ulong obj)
    {
        if (!IsServer) return;
        for (int i = 0; i < clientData.Count; i++)
        {
            if (clientData[i].ClientID == obj)
            {
                clientData.Remove(clientData[i]);
                break;
            }
        }
    }

    private void HandleClientConnected(ulong obj)
    {
        if (!IsServer) return;
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

    [Rpc(SendTo.Server, RequireOwnership = false)]
    void SendPlayerNameRpc(FixedString32Bytes playerName, RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        Debug.Log("Client: " + senderId.ToString() + " sent their name to the server");

        // Prevent duplicates
        for (int i = 0; i < clientData.Count; i++)
        {
            if (clientData[i].ClientID == senderId)
            {
                return;
            }
        }

        clientData.Add(new ClientData(senderId, playerName));
    }

    public bool IsInitialized()
    {
        return initializedOnStart && initializedOnNetworkSpawn;
    }
}
