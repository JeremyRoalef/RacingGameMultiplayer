using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        if (LobbyManager.instance != null)
        {
            LobbyManager.instance.Clients.OnListChanged -= HandleClientsListChanged;
        }
    }

    private void HandleClientsListChanged(NetworkListEvent<ulong> changeEvent)
    {
        switch (changeEvent.Type)
        {
            //New player added
            case NetworkListEvent<ulong>.EventType.Add:
                AddPlayerToUI(changeEvent.Value);
                break;
            //Player removed
            case NetworkListEvent<ulong>.EventType.Remove:
                RemovePlayerFromUI(changeEvent.Value);
                break;
        }
    }
    private void AddPlayerToUI(ulong value)
    {
        //Create the player container
        PlayerContainer newPlayerContainer = Instantiate(playerContainerPrefab, playerContainerGroup);
        newPlayerContainer.Initialize(value);

        //Store for future reference
        playerContainers.Add(newPlayerContainer);
    }
    private void RemovePlayerFromUI(ulong value)
    {
        foreach (PlayerContainer playerContainer in playerContainers)
        {
            if (playerContainer.ClientID == value)
            {
                //Remove player from container
                playerContainers.Remove(playerContainer);
                Destroy(playerContainer.gameObject);
                return;
            }
        }
    }

    public void QuitSession()
    {
        NetworkSession.QuitSession();
    }

    public void StartGame()
    {
        //Only host can start game
        if (!NetworkManager.Singleton.IsHost) return;
        NetworkSession.StartGame();
    }

    IEnumerator InitializeLobbyUI()
    {
        while (LobbyManager.instance == null)
        {
            yield return null; //No lobby manager, wait for next frame
            Debug.Log("waiting to find lobby manager");
        }

        //Lobby manager initialized
        LobbyManager.instance.Clients.OnListChanged += HandleClientsListChanged;
        
        //Initialize UI with existing clients
        foreach (ulong clientID in LobbyManager.instance.Clients)
        {
            AddPlayerToUI(clientID);
        }
    }
}
