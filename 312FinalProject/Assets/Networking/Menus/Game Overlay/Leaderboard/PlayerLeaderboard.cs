using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLeaderboard : MonoBehaviour
{
    [SerializeField]
    Image playerVehicleImage;

    [SerializeField]
    TextMeshProUGUI textPlayerPosition;

    [SerializeField]
    TextMeshProUGUI playerName;

    public void Initialize(PlayerRaceData clientData, int racePosition)
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

        textPlayerPosition.text = racePosition.ToString();
        this.playerName.text = playerName;

        //determine how to get the vehicle image later
    }
}
