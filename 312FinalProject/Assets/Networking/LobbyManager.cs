using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour, IInitializable
{
    public static LobbyManager Instance;
    public NetworkList<ClientData> clientData = new NetworkList<ClientData>();

    [SerializeField]
    NetworkObject lobbyManagerPrefab;

    bool initializedOnStart = false;
    bool initializedOnNetworkSpawn = false;

    private void Start()
    {
        //Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple lobby managers found in scene!");
        }
        
        initializedOnStart = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //Handle clients sending name to the server
        if (IsClient && !IsServer)
        {
            SendPlayerNameRpc(NetworkSession.instance.PlayerName);
        }

        if (IsServer)
        {
            //Listen to when clients disconnect
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            //Manually add host
            ulong hostId = NetworkManager.LocalClientId;

            //Add the host client data
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
        
        //Unsub from events
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }

    private void HandleClientDisconnected(ulong obj)
    {
        if (!IsServer) return;

        //Remove the data for the client who disconnected
        for (int i = 0; i < clientData.Count; i++)
        {
            if (clientData[i].ClientID == obj)
            {
                clientData.Remove(clientData[i]);
                break;
            }
        }
    }

    public void RequestToKickPlayer(ulong clientID)
    {
        if (!IsServer) return;

        KickPlayerServerRpc(clientID);
    }

    [Rpc(SendTo.Server)]
    private void KickPlayerServerRpc(ulong clientID)
    {
        //Debug.Log("kicking player");

        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientID))
        {
            NetworkManager.Singleton.DisconnectClient(clientID);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SendPlayerNameRpc(FixedString32Bytes playerName, RpcParams rpcParams = default)
    {
        //Received the sender's client ID
        ulong senderId = rpcParams.Receive.SenderClientId;

        Debug.Log("Client: " + senderId.ToString() + " sent their name to the server");

        // Prevent duplicate client IDs from existing in client data
        for (int i = 0; i < clientData.Count; i++)
        {
            if (clientData[i].ClientID == senderId)
            {
                return;
            }
        }

        //Add the client's data
        clientData.Add(new ClientData(senderId, playerName));
    }

    public bool IsInitialized() => initializedOnNetworkSpawn && initializedOnStart;
}
