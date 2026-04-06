using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerContainer : MonoBehaviour
{
    [SerializeField]
    TMP_Text playerName;

    [SerializeField]
    Button buttonKickPlayer;

    [SerializeField]
    ulong clientID;

    private void Start()
    {
        clientID = ClientID;
    }

    public ulong ClientID {  get; private set; }
    public void Initialize(ClientData clientData)
    {
        ClientID = clientData.ClientID;
        playerName.text = clientData.PlayerName.ToString();

        if (ClientID == 0 || !NetworkManager.Singleton.IsHost)
        {
            buttonKickPlayer.gameObject.SetActive(false);
        }
    }

    public void KickPlayer()
    {
        LobbyManager.Instance.RequestToKickPlayer(ClientID);
    }
}
