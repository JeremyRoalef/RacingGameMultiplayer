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

    ulong representativeClientID;

    public void Initialize(PlayerRaceData clientData, int racePosition)
    {
        textPlayerPosition.text = racePosition.ToString();
        this.playerName.text = clientData.PlayerName.ToString();
        representativeClientID = clientData.ClientID;

        //determine how to get the vehicle image later
    }
}
