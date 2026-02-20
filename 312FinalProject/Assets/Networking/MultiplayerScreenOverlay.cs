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

    static int JOIN_CODE_LENGTH = 6;
    bool joinCodeIsValid = false;

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

        buttonStartClient.interactable = false;

        buttonStartHost.onClick.AddListener(StartHostAsync);
        buttonStartClient.onClick.AddListener(StartClientAsync);
        inputFieldJoinCode.onValueChanged.AddListener(HandleJoinCodeChanged);
        inputFieldJoinCode.onValidateInput += EnsureValidJoinCodeInput;
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnects;
        }
    }

    private void OnDisable()
    {
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
        if (NetworkManager.Singleton.LocalClientId != clientID) return;

        canvas.enabled = true;
        SetInteraction(true);
    }

    private async void StartClientAsync()
    {
        //Get the join code
        string joinCode = inputFieldJoinCode.text;
        SetInteraction(false);

        await NetworkSession.StartClientAsync(
            joinCode,
            HandleClientFailedToJoinSession
            );
    }

    private async void StartHostAsync()
    {
        int numOfPlayers = 10;
        SetInteraction(false);

        await NetworkSession.StartHostAsync(
            numOfPlayers,
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
        buttonStartClient.interactable = joinCodeIsValid;
        //Display error message

    }

    private void HandleClientFailedToJoinSession(string message)
    {
        SetInteraction(true);
        buttonStartClient.interactable = joinCodeIsValid;
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
        //Invalid join code; cannot start client session
        if (newCode.Length != JOIN_CODE_LENGTH || !newCode.All(char.IsLetterOrDigit))
        {
            joinCodeIsValid = false;
            buttonStartClient.interactable = joinCodeIsValid;
        }
        else
        {
            joinCodeIsValid = true;
            buttonStartClient.interactable = joinCodeIsValid;
        }
    }
}
