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
        string playerName = string.Empty;

        //Client names aren't displaying properly. Brute force the name to show
        foreach (ClientData cd in LobbyManager.Instance.clientData)
        {
            if (cd.ClientID == clientData.ClientID)
            {
                playerName = cd.PlayerName.ToString();
            }
        }

        if (playerName == string.Empty)
        {
            Debug.LogError("Player name not found in lobby manager");
        }

        //Set the client scoreboard information
        textClientFinalRank.text = finalRankInRace.ToString();
        textClientName.text = playerName;
        textClientTime.text = clientData.TimeSpentDuringRace.ToString("F2");
    }
}
