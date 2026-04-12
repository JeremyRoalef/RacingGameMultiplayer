using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverlayCanvas : MonoBehaviour
{
    [Header("Input References")]
    [SerializeField]
    InputActionReference openPauseMenu;

    [SerializeField]
    InputActionReference closePauseMenu;

    [Header("Object References")]
    [SerializeField]
    FinalScoreboard finalScoreboard;

    [SerializeField]
    GameObject leaderboardPanel;

    [SerializeField]
    PausePanel pausePanel;

    private void Awake()
    {
        finalScoreboard.gameObject.SetActive(false);
        leaderboardPanel.SetActive(true);
    }

    private void OnEnable()
    {
        //Subscribe to events
        StartCoroutine(SubscribeToRaceManager());
        openPauseMenu.action.performed += OpenPauseMenu;
        closePauseMenu.action.performed += ClosePauseMenu;
    }

    private void OnDisable()
    {
        //Unsubscribe from events
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.OnRaceFinished -= HandleRaceFinished;
        }
        openPauseMenu.action.performed -= OpenPauseMenu;
        closePauseMenu.action.performed -= ClosePauseMenu;
    }

    public void OnButtonResumeGameClicked() => ClosePauseMenu();
    public void OnButtonQuitGameClicked() => NetworkSession.QuitSession();
    public void OnButtonContinueClicked()
    {
        //Return to waiting for clients scene
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkSession.ReturnToWaitingForClientsScene();
        }
    }

    private void HandleRaceFinished()
    {
        //Display the final scoreboard
        finalScoreboard.gameObject.SetActive(true);
        List<PlayerRaceData> playerRaceData = new List<PlayerRaceData>();

        //Get the player race data from the race manager (as a list)
        foreach (PlayerRaceData raceData in RaceManager.Instance.playerRaceData)
        {
            playerRaceData.Add(raceData);
        }

        //Pass the information to the scoreboard for processing
        finalScoreboard.SetScoreboardInformation(playerRaceData);

        //Hide the leaderboard panel (no longer needed)
        leaderboardPanel.SetActive(false);
    }

    void ClosePauseMenu(InputAction.CallbackContext context) => ClosePauseMenu();

    void ClosePauseMenu()
    {
        pausePanel.TogglePausePanel(false);
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        playerInput.SwitchCurrentActionMap("Player");
    }

    void OpenPauseMenu(InputAction.CallbackContext context)
    {
        pausePanel.TogglePausePanel(true);
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        playerInput.SwitchCurrentActionMap("UI");
    }

    IEnumerator SubscribeToRaceManager()
    {
        //Wait for the race manager to load
        while (RaceManager.Instance == null)
        {
            yield return null;
        }

        RaceManager.Instance.OnRaceFinished += HandleRaceFinished;
    }
}
