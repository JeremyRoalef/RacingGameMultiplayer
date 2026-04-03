using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RaceManager : NetworkBehaviour
{
    public Action OnClientAdded;
    public Action OnClientHitCheckpoint;
    public Action OnRaceInitialized;
    public Action<ulong> OnClientFinishedRace;
    public Action OnRaceFinished;
    public Action OnRaceStart;

    [SerializeField]
    NetworkObject playerPrefab;

    static int TOTAL_LAPS = 3;

    List<ulong> clientsWhoFinishedRace = new List<ulong>();

    public static RaceManager Instance { get; private set; }
    List<Transform> availableSpawnPoints = new List<Transform>();
    public NetworkList<PlayerRaceData> playerRaceData = new NetworkList<PlayerRaceData>();
    //Dictionary<ulong, PlayerRaceData> playerRaceData = new Dictionary<ulong, PlayerRaceData>();
    float timeWhenRaceStarted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple race managers found in scene!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!IsServer) return;

        //Get & store spawn points
        SpawnPoints spawnPoints = FindFirstObjectByType<SpawnPoints>();
        foreach (Transform child in spawnPoints.transform)
        {
            availableSpawnPoints.Add(child);
        }

        //Spawn clients
        StartCoroutine(SpawnClients());

        //Handle client disconnection
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

        Debug.Log(TOTAL_LAPS);
    }

    private void OnDisable()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        //Server control
        if (!IsServer) return;

        base.OnNetworkSpawn();
    }

    IEnumerator SpawnClients()
    {
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting to spawn players");
            yield return null;
        }

        foreach (KeyValuePair<ulong, NetworkClient> clientKeyValue in NetworkManager.Singleton.ConnectedClients)
        {
            if (availableSpawnPoints.Count  == 0)
            {
                Debug.LogError("Error: Not enough spawn points for current session!");
            }

            //Get spawn position
            Transform spawnPos = availableSpawnPoints.First();
            availableSpawnPoints.Remove(spawnPos);

            //Create & Assign player prefab
            NetworkObject newPlayer = Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
            newPlayer.SpawnAsPlayerObject(clientKeyValue.Key);

            //Create player's race data
            PlayerRaceData raceData = new PlayerRaceData(
                    clientKeyValue.Key,
                    clientKeyValue.Key.ToString(),
                    0,
                    0,
                    0,
                    false
                );
            playerRaceData.Add(raceData);

            //tell any listeners about the new client addition
            OnClientAddedRPC(raceData);
        }

        //Race has been initialized
        OnRaceInitializedRPC();
    }

    public void HandleClientHitCheckpoint(ulong clientID, int checkpointIndex)
    {
        //Update the client data in the list
        for (int i = 0; i < playerRaceData.Count; i++)
        {
            //Check if it's the right client ID
            if (playerRaceData[i].ClientID != clientID) continue;
            
            //Get the client's data
            PlayerRaceData clientData = playerRaceData[i];
            DebugMessageClientRpc($"{clientData.PlayerName} reached checkpoint {checkpointIndex}");

            //Check if client lapped or finished race
            bool clientLapped = checkpointIndex == 0;
            bool finishedRace = clientLapped && clientData.CompletedLaps == TOTAL_LAPS - 1;

            //Set the client's new checkpoint data
            PlayerRaceData newClientData = new PlayerRaceData(
                clientID,
                clientData.PlayerName,
                checkpointIndex,
                clientLapped ? clientData.CompletedLaps + 1 : clientData.CompletedLaps,
                Time.time - timeWhenRaceStarted,
                finishedRace
                );

            //Assign the client's new race data
            playerRaceData[i] = newClientData;

            if (finishedRace)
            {
                //Handle client finished race
                clientsWhoFinishedRace.Add(clientID);
                OnClientFinishedRaceRPC(clientID);
                DebugMessageClientRpc($"{newClientData.PlayerName} finished race");

                //Check if all clients have finished the race
                if (clientsWhoFinishedRace.Count >= NetworkManager.Singleton.ConnectedClients.Count)
                {
                    OnRaceFinishedRPC();
                }
            }

            DebugMessageClientRpc($"{newClientData.PlayerName} completed laps: {newClientData.CompletedLaps}");
            OnClientHitCheckpointRPC(newClientData);
            break;
        }
    }

    private void HandleClientDisconnected(ulong clientID)
    {
        bool foundClientData = false;
        PlayerRaceData clientData = PlayerRaceData.Invalid;
        foreach (PlayerRaceData playerData in playerRaceData)
        {
            if (playerData.ClientID == clientID)
            {
                //Initialize the client data & continue
                foundClientData = true;
                clientData = playerData;
                continue;
            }
        }
        if (!foundClientData) return;

        playerRaceData.Remove(clientData);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DebugMessageClientRpc(string message)
    {
        Debug.Log(message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnClientHitCheckpointRPC(PlayerRaceData clientData)
    {
        OnClientHitCheckpoint?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnClientAddedRPC(PlayerRaceData clientData)
    {
        OnClientAdded?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnRaceInitializedRPC()
    {
        Debug.Log("Race initialized");
        OnRaceInitialized?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnRaceFinishedRPC()
    {
        OnRaceFinished?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnClientFinishedRaceRPC(ulong clientId)
    {
        OnClientFinishedRace?.Invoke(clientId);
    }
}