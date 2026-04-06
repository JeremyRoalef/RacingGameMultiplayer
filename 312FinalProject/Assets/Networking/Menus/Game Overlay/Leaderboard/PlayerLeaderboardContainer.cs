using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLeaderboardContainer : MonoBehaviour
{
    [SerializeField]
    Transform clientLeaderboardContainter;

    [SerializeField]
    PlayerLeaderboard clientLeaderboardPrefab;

    private void OnDisable()
    {
        if (RaceManager.Instance != null)
        {
            //Race manager is initialized
            RaceManager.Instance.OnClientAdded -= HandleClientAdded;
            RaceManager.Instance.OnClientHitCheckpoint -= HandleClientHitCheckpoint;
            RaceManager.Instance.OnRaceInitialized -= CreateLeaderboard;
        }
    }

    private void Awake()
    {
        StartCoroutine(InitializeLeaderboardContainer());
    }

    private void HandleClientHitCheckpoint()
    {
        //Update the leaderboard to display any changes necessary in the order. Note that the order is determined by the child's
        //alignment in the leaderboard container

        CreateLeaderboard();
    }

    private void HandleClientAdded()
    {
        //Add a new client to the leaderbaord container & store it in the list
        CreateLeaderboard();
    }

    void HandleClientRemoved()
    {
        CreateLeaderboard();
    }

    IEnumerator InitializeLeaderboardContainer()
    {
        while (RaceManager.Instance == null)
        {
            //Wait until the race manager is initialized
            yield return null;
        }

        //Race manager is initialized
        RaceManager.Instance.OnClientAdded += HandleClientAdded;
        RaceManager.Instance.OnClientRemoved += HandleClientRemoved;
        RaceManager.Instance.OnClientHitCheckpoint += HandleClientHitCheckpoint;
        
        while (NetworkManager.Singleton == null)
        {
            //Wait for networkmanager to be initialized
            yield return null;
        }

        //Wait for next frame to initialize the leaderboard
        yield return null;
        CreateLeaderboard();
    }

    private void CreateLeaderboard()
    {
        Debug.Log("Creating the leaderboard");
        List<PlayerRaceData> playerScoreboard = new List<PlayerRaceData>();

        //clear out the existing leaderboard
        foreach(Transform child in clientLeaderboardContainter)
        {
            Destroy(child.gameObject);
        }

        //Create new scoreboard
        foreach (PlayerRaceData playerData in RaceManager.Instance.playerRaceData)
        {
            Debug.Log("Player data: " + playerData.ToString());
            playerScoreboard.Add(playerData);
        }

        //Sort the entries in the list based on the laps each entry completed, then by the checkpoint hit
        playerScoreboard = playerScoreboard.
            OrderByDescending(x => x.CompletedLaps).
            ThenByDescending(x => x.CurrentCheckpointIndex).ToList();

        //Debugging
        Debug.Log("Scoreboard sorted: ");
        foreach (PlayerRaceData data in playerScoreboard)
        {
            Debug.Log(data.ToString());
        }

        //Create the new leaderboard elements
        for (int clientRacePosition = 1; clientRacePosition <= playerScoreboard.Count; clientRacePosition++)
        {
            //Get the client data (position is 1 ahead of their index position)
            PlayerRaceData clientData = playerScoreboard[clientRacePosition - 1];
            
            //Create the new client leaderboard entry
            PlayerLeaderboard clientLeaderboardObj = Instantiate(clientLeaderboardPrefab, clientLeaderboardContainter);
            clientLeaderboardObj.Initialize(clientData, clientRacePosition);
        }

        Debug.Log("Leaderboard created");
    }
}
