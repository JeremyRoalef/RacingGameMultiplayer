using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverlayCanvas : MonoBehaviour
{
    [SerializeField]
    InputActionReference openPauseMenu;

    [SerializeField]
    InputActionReference closePauseMenu;

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
        StartCoroutine(SubscribeToRaceManager());

        openPauseMenu.action.performed += OpenPauseMenu;
        closePauseMenu.action.performed += ClosePauseMenu;
    }

    private void OnDisable()
    {
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.OnRaceFinished -= HandleRaceFinished;
        }
        openPauseMenu.action.performed -= OpenPauseMenu;
        closePauseMenu.action.performed -= ClosePauseMenu;
    }

    private void HandleRaceFinished()
    {
        Debug.Log("Race is finished on overlay canvas");

        //Display the final scoreboard
        finalScoreboard.gameObject.SetActive(true);
        List<PlayerRaceData> playerRaceData = new List<PlayerRaceData>();

        //Get the player race data from the race manager (as a list)
        foreach(PlayerRaceData raceData in RaceManager.Instance.playerRaceData)
        {
            Debug.Log("Adding race data to list of player data");
            playerRaceData.Add(raceData);
        }

        //Pass the information to the scoreboard for processing
        finalScoreboard.SetScoreboardInformation(playerRaceData);

        //Hide the leaderboard panel (no longer needed)
        leaderboardPanel.SetActive(false);
    }

    private void ClosePauseMenu(InputAction.CallbackContext context)
    {
        ClosePauseMenu();
    }

    void ClosePauseMenu()
    {
        pausePanel.TogglePausePanel(false);
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        playerInput.SwitchCurrentActionMap("Player");
    }

    private void OpenPauseMenu(InputAction.CallbackContext context)
    {
        pausePanel.TogglePausePanel(true);
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        playerInput.SwitchCurrentActionMap("UI");
    }

    IEnumerator SubscribeToRaceManager()
    {
        while (RaceManager.Instance == null)
        {
            yield return null;
        }

        RaceManager.Instance.OnRaceFinished += HandleRaceFinished;
    }

    public void OnButtonResumeGameClicked()
    {
        ClosePauseMenu();
    }

    public void OnButtonQuitGameClicked()
    {
        NetworkSession.QuitSession();
    }
}
