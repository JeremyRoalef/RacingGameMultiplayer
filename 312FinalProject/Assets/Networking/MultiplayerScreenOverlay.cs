using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerScreenOverlay : MonoBehaviour
{
    [SerializeField]
    Canvas canvas;

    [SerializeField]
    Button buttonStartHost;

    [SerializeField]
    Button buttonStartClient;

    [SerializeField]
    TMP_InputField inputFieldJoinCode;

    static int NUM_OF_PLAYERS = 10;
    static int JOIN_CODE_LENGTH = 6;
    bool isValidJoinCode = false;
    bool isQuittingApplication = false;

    private void Awake()
    {
        //Internal Setup check
        if (
            buttonStartClient == null || 
            buttonStartHost == null || 
            inputFieldJoinCode == null
            )
        {
            Debug.LogError("Error: internal config not setup");
            return;
        }

        //player should not be able to start as client by default
        buttonStartClient.interactable = false;
    }

    private void OnEnable()
    {
        //Subscribe to events
        buttonStartHost.onClick.AddListener(StartHostAsync);
        buttonStartClient.onClick.AddListener(StartClientAsync);
        inputFieldJoinCode.onValueChanged.AddListener(HandleJoinCodeChanged);
        inputFieldJoinCode.onValidateInput += EnsureValidJoinCodeInput;
        StartCoroutine(ListenToNetworkManager());
    }

    private void OnDisable()
    {
        //Unsubscribe from events
        buttonStartHost.onClick.RemoveListener(StartHostAsync);
        buttonStartClient.onClick.RemoveListener(StartClientAsync);
        inputFieldJoinCode.onValueChanged.RemoveListener(HandleJoinCodeChanged);
        inputFieldJoinCode.onValidateInput -= EnsureValidJoinCodeInput;
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnects;
        }
    }

    private void HandleClientDisconnects(ulong clientID)
    {
        //Only this player should care about itself disconnecting
        if (NetworkManager.Singleton.LocalClientId != clientID) return;

        //Show the screen overlay & make it interactable
        canvas.enabled = true;
        SetInteraction(true);
    }

    private async void StartClientAsync()
    {
        //Get the join code
        string joinCode = inputFieldJoinCode.text;

        //Canvas should no longer be interacted with
        SetInteraction(false);

        //Tell the network session to start as a client
        await NetworkSession.StartClientAsync(
            joinCode,
            HandleClientFailedToJoinSession
            );
    }

    private async void StartHostAsync()
    {
        SetInteraction(false);

        //Tell the network session to start as a host
        await NetworkSession.StartHostAsync(
            NUM_OF_PLAYERS,
            HandleHostSessionFailed
            );
    }

    void SetInteraction(bool interactable)
    {
        buttonStartClient.interactable = interactable;
        buttonStartHost.interactable = interactable;
        inputFieldJoinCode.interactable = interactable;
    }

    void HandleHostSessionFailed(string message)
    {
        SetInteraction(true);
        buttonStartClient.interactable = isValidJoinCode;
        //Display error message

    }

    private void HandleClientFailedToJoinSession(string message)
    {
        SetInteraction(true);
        buttonStartClient.interactable = isValidJoinCode;
        //Display error message
    }

    private char EnsureValidJoinCodeInput(string text, int charIndex, char addedChar)
    {
        if (charIndex >= JOIN_CODE_LENGTH || !char.IsLetterOrDigit(addedChar))
        {
            //Character rejection; doesn't fit join code criteria
            return '\0';
        }
        else
        {
            return char.ToUpper(addedChar);
        }
    }

    private void HandleJoinCodeChanged(string newCode)
    {
        //if Invalid join code, cannot start client session
        if (newCode.Length != JOIN_CODE_LENGTH || !newCode.All(char.IsLetterOrDigit))
        {
            isValidJoinCode = false;
            buttonStartClient.interactable = isValidJoinCode;
        }
        else
        {
            isValidJoinCode = true;
            buttonStartClient.interactable = isValidJoinCode;
        }
    }

    public void OnButtonQuitClicked()
    {
        //Do not make multiple application quit calls
        if (isQuittingApplication) return;

        Debug.Log("Quitting Application");

        isQuittingApplication = true;

        //Shudown the network manager
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        //Exit the application
        Application.Quit();
    }

    IEnumerator ListenToNetworkManager()
    {
        //Wait for the network manager to load
        while (NetworkManager.Singleton == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        //Subscribe to network manager events
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnects;
    }
}
