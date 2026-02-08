using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject mainMenuContainer;
    [SerializeField] private GameObject failContainer;
    [SerializeField] private GameObject winContainer;
    [SerializeField] private GameObject timerContainer;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI lastCheckPointTimeText;

    [SerializeField] private TextMeshProUGUI winTimeText;

    private const int EASY = 1;
    private const int MED = 2;
    private const int HARD = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ShowMainMenu();
    }

    // Methods for main menu buttons
    public void OnClickEasy()
    {
        StartGame(EASY);
    }

    public void OnClickMedium()
    {
        StartGame(MED);
    }

    public void OnClickHard()
    {
        StartGame(HARD);
    }

    public void OnClickQuit()
    {
        Application.Quit(); // Closes the application
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stops the unity editor
        #endif
    }

    // Start a game with the set difficulty
    private void StartGame(int difficulty)
    {
        Debug.Log("Starting Game with difficulty " + difficulty);
        // Pass difficulty to RaceManager or other scripts
        StartCoroutine(CreateTrack.Instance.BuildTrack(difficulty));

        // Hide all overlays
        mainMenuContainer.SetActive(false);
        failContainer.SetActive(false);
        winContainer.SetActive(false);

        RaceManager.Instance.StartRace();
        timerContainer.SetActive(true);
    }

    // Fail Overlay
    public void ShowFail()
    {
        timerContainer.SetActive(false);
        failContainer.SetActive(true);
    }

    public void OnRetryFromFail() // Retry button
    {
        failContainer.SetActive(false);
        RaceManager.Instance.ResetRaceAndStart();
        timerContainer.SetActive(true);
    }

    public void OnMainMenuFromFail() // Main Menu button
    {
        failContainer.SetActive(false);
        ShowMainMenu();
        CreateTrack.Instance.ClearTrack();
        RaceManager.Instance.ResetRaceStateOnly();
    }

    // Race win overlay
    public void ShowWin(float finalTime)
    {
        timerContainer.SetActive(false);
        winTimeText.text = $"Final Time: {finalTime:0.00}s";
        winContainer.SetActive(true);
    }

    public void OnRetryFromWin() // retry button
    {
        winContainer.SetActive(false);
        RaceManager.Instance.ResetRaceAndStart();
        timerContainer.SetActive(true);
    }

    public void OnMainMenuFromWin() // main menu button
    {
        winContainer.SetActive(false);
        ShowMainMenu();
        CreateTrack.Instance.ClearTrack();
        RaceManager.Instance.ResetRaceStateOnly();
    }

    // When returning to the main menu
    private void ShowMainMenu()
    {
        // Clear the track
        CreateTrack.Instance.ClearTrack();

        // Reset active overlays
        mainMenuContainer.SetActive(true);
        failContainer.SetActive(false);
        winContainer.SetActive(false);
        timerContainer.SetActive(false);
    }

    // Update live in-game timers
    public void UpdateTimers(float totalTime, float lastCheckpointTime)
    {
        totalTimeText.text = $"Total Time: {FormatTime(totalTime)}";
        lastCheckPointTimeText.text = $"Last Checkpoint Time: {FormatTime(lastCheckpointTime)}";
    }

    // Format raw unity time to readable time. 
    private string FormatTime(float time)
    {
        int minutes = (int)time / 60;
        float seconds = (int)(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}
