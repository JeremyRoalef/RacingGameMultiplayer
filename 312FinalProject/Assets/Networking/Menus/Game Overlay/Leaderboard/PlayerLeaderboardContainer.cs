using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
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
            RaceManager.Instance.OnClientAdded -= CreateLeaderboard;
            RaceManager.Instance.OnClientRemoved -= CreateLeaderboard;
            RaceManager.Instance.OnClientHitCheckpoint -= CreateLeaderboard;
        }
    }

    private void Awake()
    {
        StartCoroutine(InitializeLeaderboardContainer());
    }

    IEnumerator InitializeLeaderboardContainer()
    {
        //Wait until the race manager loads
        while (RaceManager.Instance == null)
        {
            yield return null;
        }

        //Wait for the race manager to be initialized
        while (!RaceManager.Instance.IsInitialized())
        {
            yield return null;
        }

        //Race manager is initialized
        RaceManager.Instance.OnClientAdded += CreateLeaderboard;
        RaceManager.Instance.OnClientRemoved += CreateLeaderboard;
        RaceManager.Instance.OnClientHitCheckpoint += CreateLeaderboard;

        //Wait for networkmanager to be load
        while (NetworkManager.Singleton == null)
        {
            yield return null;
        }

        //Wait for next frame to initialize the leaderboard, just in case
        yield return null;
        CreateLeaderboard();
    }

    private void CreateLeaderboard()
    {
        //Debug.Log("Creating the leaderboard");
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

        //Debug.Log("Leaderboard created");
    }
}
