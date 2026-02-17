using System;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager instance;
    public NetworkList<ulong> Clients = new NetworkList<ulong>();

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

        //GetComponent<NetworkObject>().Spawn();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Spawned over network");

        if (!IsServer) return;
        base.OnNetworkSpawn();

        //Manually add host
        Clients.Add(NetworkManager.LocalClientId);

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        base.OnNetworkDespawn();
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
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
}
