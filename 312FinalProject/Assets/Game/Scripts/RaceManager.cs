using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : NetworkBehaviour, IInitializable
{
    public static RaceManager Instance { get; private set; }

    public Action OnClientAdded;
    public Action OnClientRemoved;
    public Action OnClientHitCheckpoint;
    public Action<ulong> OnClientFinishedRace;
    public Action OnRaceFinished;

    public NetworkList<PlayerRaceData> playerRaceData = new NetworkList<PlayerRaceData>();
    NetworkVariable<bool> isInitialized = new NetworkVariable<bool>(false);

    [SerializeField]
    NetworkObject playerPrefab;

    public List<ulong> clientsWhoFinishedRace = new List<ulong>();
    List<Transform> availableSpawnPoints = new List<Transform>();
    List<NetworkObject> clientObjectsInGame = new List<NetworkObject>();

    float timeWhenRaceStarted;
    static int TOTAL_LAPS = 1;

    private void Awake()
    {
        //Singleton Pattern
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

        //Handle client connection/disconnection
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

        //Handle scene changes
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += CleanupRaceSession;

        Debug.Log(TOTAL_LAPS);
    }

    private void OnDisable()
    {
        if (IsServer)
        {
            //Cleanup subscription events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= CleanupRaceSession;
            }
        }
    }

    public string GetClientName(ulong clientID)
    {
        //Find the client's data in the lobby manager
        foreach (ClientData cd in LobbyManager.Instance.clientData)
        {
            if (cd.ClientID == clientID)
            {
                return cd.PlayerName.ToString();
            }
        }

        Debug.Log("Client not found in list");
        return "";
    }

    public bool IsInitialized() => isInitialized.Value;

    public void HandleClientHitCheckpoint(ulong clientID, int checkpointIndex)
    {
        //Update the client data in the list
        for (int i = 0; i < playerRaceData.Count; i++)
        {
            //Check if it's the right client ID
            if (playerRaceData[i].ClientID != clientID) continue;
            
            //Get the client's data
            PlayerRaceData clientData = playerRaceData[i];
            //DebugMessageClientRpc($"{clientData.PlayerName} reached checkpoint {checkpointIndex}");

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
                //DebugMessageClientRpc($"{newClientData.PlayerName} finished race");

                //Check if all clients have finished the race
                //DebugMessageClientRpc("Clients who finished race: " + clientsWhoFinishedRace.Count);
                if (clientsWhoFinishedRace.Count >= NetworkManager.Singleton.ConnectedClients.Count)
                {
                    //DebugMessageClientRpc("All clients finished race, OnRaceFinishedRPC");
                    OnRaceFinishedRPC();
                }
            }

            //DebugMessageClientRpc($"{newClientData.PlayerName} completed laps: {newClientData.CompletedLaps}");
            OnClientHitCheckpointRPC(newClientData);
            break;
        }
    }

    private void HandleClientDisconnected(ulong clientID)
    {
        bool foundClientData = false;
        PlayerRaceData clientData = PlayerRaceData.Invalid;

        //Try to find the client's race data
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

        //Client data not found, don't do anything
        if (!foundClientData) return;

        //Remove the client's data
        playerRaceData.Remove(clientData);
        OnClientRemovedRPC(clientData);
    }

    private void CleanupRaceSession(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        //Don't deconstruct race session if you're in the race session
        if (sceneName == "PreBuiltLevel") { return; }

        //Despawn all network object player prefabs
        foreach (NetworkObject clientObj in clientObjectsInGame)
        {
            if (clientObj == null) continue;

            //Despawn the client's object (no longer needed when leaving the race scene)
            clientObj.Despawn();
        }

        //Despawn this race manager
        NetworkObject thisNetworkObj = GetComponent<NetworkObject>();
        thisNetworkObj.Despawn();
    }

    private void HandleClientConnected(ulong clientID)
    {
        //Handle edge cases where client is already connected, but calls this method
        foreach (PlayerRaceData clientData in playerRaceData)
        {
            if (clientData.ClientID == clientID)
            {
                //Player already exists. Do not handle them connecting
                return;
            }
        }

        //TOOD: let clients reuse spawn points
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("Error: Not enough spawn points for current session!");
        }

        //Get spawn position
        Transform spawnPos = availableSpawnPoints.First();
        availableSpawnPoints.Remove(spawnPos);

        //Create & Assign player prefab
        NetworkObject newPlayer = Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
        newPlayer.SpawnAsPlayerObject(clientID);
        clientObjectsInGame.Add(newPlayer);

        //Create player's race data
        PlayerRaceData raceData = new PlayerRaceData(
                clientID,
                GetClientName(clientID),
                0,
                0,
                0,
                false
            );
        playerRaceData.Add(raceData);

        //tell any listeners about the new client addition
        OnClientAddedRPC(raceData);
    }

    IEnumerator SpawnClients()
    {
        //Wait for the network manager singleton to load
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting to spawn players");
            yield return null;
        }

        //Wait for the lobby manager to load and initialize
        while (LobbyManager.Instance == null || !LobbyManager.Instance.IsInitialized())
        {
            yield return null;
        }

        //Wait for all client's data to sync from the lobby manager
        while (LobbyManager.Instance.clientData.Count < NetworkManager.Singleton.ConnectedClients.Count)
        {
            yield return null;
        }

        //Everything is ready; spawn the clients
        foreach (KeyValuePair<ulong, NetworkClient> clientKeyValue in NetworkManager.Singleton.ConnectedClients)
        {
            //TODO: handle many clients trying to use spawn points
            if (availableSpawnPoints.Count == 0)
            {
                Debug.LogWarning("Error: Not enough spawn points for current session!");
            }

            //Get spawn position
            Transform spawnPos = availableSpawnPoints.First();
            availableSpawnPoints.Remove(spawnPos);

            //Create & Assign player prefab
            NetworkObject newPlayer = Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
            newPlayer.SpawnAsPlayerObject(clientKeyValue.Key);
            clientObjectsInGame.Add(newPlayer);

            //Create player's race data
            PlayerRaceData raceData = new PlayerRaceData(
                    clientKeyValue.Key,
                    GetClientName(clientKeyValue.Key),
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
        isInitialized.Value = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DebugMessageClientRpc(string message)
    {
        Debug.Log(message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnClientHitCheckpointRPC(PlayerRaceData clientData) => OnClientHitCheckpoint?.Invoke();

    [Rpc(SendTo.ClientsAndHost)]
    void OnClientAddedRPC(PlayerRaceData clientData) => OnClientAdded?.Invoke();

    [Rpc(SendTo.ClientsAndHost)]
    void OnClientRemovedRPC(PlayerRaceData clientData) => OnClientRemoved?.Invoke();

    [Rpc(SendTo.ClientsAndHost)]
    void OnRaceFinishedRPC() => OnRaceFinished?.Invoke();

    [Rpc(SendTo.ClientsAndHost)]
    void OnClientFinishedRaceRPC(ulong clientId)
    {
        //Update the clients who finished race on everyone but the host
        if (!IsHost)
        {
            clientsWhoFinishedRace.Add(clientId);
        }

        //Handle the client finishing the race locally
        OnClientFinishedRace?.Invoke(clientId);
    }
}