using TMPro;
using UnityEngine;

public class ClientScoreboardContainer : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI textClientFinalRank;

    [SerializeField]
    TextMeshProUGUI textClientName;

    [SerializeField]
    TextMeshProUGUI textClientTime;

    public void Initialize(PlayerRaceData clientData, int finalRankInRace)
    {
        //Set the client scoreboard information
        textClientFinalRank.text = finalRankInRace.ToString();
        textClientName.text = clientData.PlayerName.ToString();
        textClientTime.text = clientData.TimeSpentDuringRace.ToString("F2");
    }
}
