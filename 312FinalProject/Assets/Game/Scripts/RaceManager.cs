using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RaceManager : NetworkBehaviour
{
    public Action OnRaceStart;

    [SerializeField]
    NetworkObject playerPrefab;

    [SerializeField]
    NetworkVariable<int> totalLaps;

    public static RaceManager Instance { get; private set; }
    List<Transform> availableSpawnPoints = new List<Transform>();
    Dictionary<ulong, PlayerRaceData> playerRaceData = new Dictionary<ulong, PlayerRaceData>();

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

        Debug.Log(totalLaps.Value);
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
                    clientKeyValue.Key.ToString(),
                    0,
                    0,
                    0,
                    false
                );
            playerRaceData.Add(clientKeyValue.Key, raceData);
        }
    }

    public void HandleClientHitCheckpoint(ulong clientID, int checkpointIndex)
    {
        PlayerRaceData clientData = playerRaceData[clientID];
        if (clientData == null)
        {
            return;
        }

        DebugMessageClientRpc($"{clientData.PlayerName} reached checkpoint {checkpointIndex}");

        clientData.SetCheckpointIndex(checkpointIndex);
        if (clientData.CompletedLaps == totalLaps.Value)
        {
            clientData.FinishedRace = true;
            DebugMessageClientRpc($"{clientData.PlayerName} finished race");
        }

        DebugMessageClientRpc($"{clientData.PlayerName} completed laps: {clientData.CompletedLaps}");
    }

    private void HandleClientDisconnected(ulong clientID)
    {
        playerRaceData.Remove(clientID);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DebugMessageClientRpc(string message)
    {
        Debug.Log(message);
    }
}