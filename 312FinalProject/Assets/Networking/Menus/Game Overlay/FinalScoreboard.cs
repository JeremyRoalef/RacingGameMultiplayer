using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinalScoreboard : MonoBehaviour
{
    [SerializeField]
    ClientScoreboardContainer clientScoreboardPrefab;

    [SerializeField]
    Transform scoreboardContainer;

    public void SetScoreboardInformation(List<PlayerRaceData> playerRaceData)
    {
        Debug.Log("Adding player race data to the scoreboard");

        //Sort the player race data by the time it took for the client to finish the race. This gives the rank of the users
        playerRaceData = playerRaceData.OrderByDescending(x => x.TimeSpentDuringRace).ToList();
        //Data is currently from last to first. Reverse such that it is first to last
        playerRaceData.Reverse();

        for (int i = 1; i <= playerRaceData.Count; i++)
        {
            //Craete the client scoreboards in the scoreboard container
            ClientScoreboardContainer clientScoreboardObj = Instantiate(clientScoreboardPrefab, scoreboardContainer);

            Debug.Log("Added new player data");

            //Set the client's data
            clientScoreboardObj.Initialize(playerRaceData[i - 1], i);
        }
    }
}
