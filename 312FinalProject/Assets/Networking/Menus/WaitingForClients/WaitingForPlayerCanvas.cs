using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayerCanvas : MonoBehaviour
{
    [SerializeField]
    PlayerContainer playerContainerPrefab;

    [SerializeField]
    Transform playerContainerGroup;

    [SerializeField]
    TMP_Text joinCodeText;

    [SerializeField]
    Button buttonStart;

    List<PlayerContainer> playerContainers = new List<PlayerContainer>();

    private void Start()
    {
        StartCoroutine(InitializeLobbyUI());

        //Display join code
        joinCodeText.text = NetworkSession.instance.JoinCode;

        //Handle client start button visibility
        if (!NetworkManager.Singleton.IsServer)
        {
            buttonStart.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        //subscription expired
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.clientData.OnListChanged -= HandleClientsListChanged;
        }
    }

    private void HandleClientsListChanged(NetworkListEvent<ClientData> changeEvent)
    {
        switch (changeEvent.Type)
        {
            //New player added
            case NetworkListEvent<ClientData>.EventType.Add:
                AddPlayerToUI(changeEvent.Value);
                break;
            //Player removed
            case NetworkListEvent<ClientData>.EventType.Remove:
                RemovePlayerFromUI(changeEvent.Value);
                break;
        }
    }

    private void AddPlayerToUI(ClientData clientData)
    {
        //Create the player container
        PlayerContainer newPlayerContainer = Instantiate(playerContainerPrefab, playerContainerGroup);
        newPlayerContainer.Initialize(clientData);

        //Store for future reference
        playerContainers.Add(newPlayerContainer);
    }
    private void RemovePlayerFromUI(ClientData clientData)
    {
        foreach (PlayerContainer playerContainer in playerContainers)
        {
            if (playerContainer.ClientID == clientData.ClientID)
            {
                //Remove player from container
                playerContainers.Remove(playerContainer);
                Destroy(playerContainer.gameObject);
                return;
            }
        }
    }

    public void QuitSession() => NetworkSession.QuitSession();

    public void StartGame()
    {
        //Only host can start game
        if (!NetworkManager.Singleton.IsHost) return;
        NetworkSession.StartGame();
    }

    IEnumerator InitializeLobbyUI()
    {
        while (LobbyManager.Instance == null)
        {
            yield return null; //No lobby manager, wait for next frame
            Debug.Log("waiting to find lobby manager");
        }

        //Lobby manager initialized
        LobbyManager.Instance.clientData.OnListChanged += HandleClientsListChanged;
        
        //Initialize UI with existing clients
        foreach (ClientData clientData in LobbyManager.Instance.clientData)
        {
            AddPlayerToUI(clientData);
        }
    }
}
