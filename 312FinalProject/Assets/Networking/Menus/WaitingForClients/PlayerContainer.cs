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
    public void Initialize(ulong clientID)
    {
        ClientID = clientID;
        playerName.text = ClientID.ToString();

        if (ClientID == 0 || !NetworkManager.Singleton.IsHost)
        {
            buttonKickPlayer.gameObject.SetActive(false);
        }
    }

    public void KickPlayer()
    {
        LobbyManager.instance.RequestToKickPlayer(ClientID);
    }
}
