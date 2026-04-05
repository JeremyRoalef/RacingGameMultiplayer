using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverlayCanvas : MonoBehaviour
{
    [SerializeField]
    FinalScoreboard finalScoreboard;

    [SerializeField]
    GameObject leaderboardPanel;

    private void Awake()
    {
        finalScoreboard.gameObject.SetActive(false);
        leaderboardPanel.SetActive(true);
    }

    private void OnEnable()
    {
        StartCoroutine(SubscribeToRaceManager());
    }

    private void OnDisable()
    {
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.OnRaceFinished -= HandleRaceFinished;
        }
    }

    private void HandleRaceFinished()
    {
        Debug.Log("Race is finished on overlay canvas");

        //Display the final scoreboard
        finalScoreboard.gameObject.SetActive(true);
        List<PlayerRaceData> playerRaceData = new List<PlayerRaceData>();

        //Get the player race data from the race manager (as a list)
        foreach(PlayerRaceData raceData in RaceManager.Instance.playerRaceData)
        {
            Debug.Log("Adding race data to list of player data");
            playerRaceData.Add(raceData);
        }

        //Pass the information to the scoreboard for processing
        finalScoreboard.SetScoreboardInformation(playerRaceData);

        //Hide the leaderboard panel (no longer needed)
        leaderboardPanel.SetActive(false);
    }

    IEnumerator SubscribeToRaceManager()
    {
        while (RaceManager.Instance == null)
        {
            yield return null;
        }

        RaceManager.Instance.OnRaceFinished += HandleRaceFinished;
    }
}
