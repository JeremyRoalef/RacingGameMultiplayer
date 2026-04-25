using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitCanvas : MonoBehaviour, IInitializable
{
    [SerializeField]
    Button buttonSubmitName;

    [SerializeField]
    TMP_InputField inputFieldPlayerName;

    bool isInitialized = false;

    private void Awake()
    {
        inputFieldPlayerName.onValueChanged.AddListener(HandlePlayerNameChanged);
        buttonSubmitName.enabled = false;
    }

    private void HandlePlayerNameChanged(string newName)
    {
        if (newName.Length == 0)
        {
            //Name is not valid; no name given
            buttonSubmitName.enabled = false;
        }
        else
        {
            //Very laxed rules; name can be anything that isn't empty
            buttonSubmitName.enabled = true;
        }
    }

    public void OnButtonSubmitNameClicked()
    {
        //Get the name from the input field
        string playerName = inputFieldPlayerName.text;
        NetworkSession.SetPlayerName(playerName);
        isInitialized = true;
    }

    public bool IsInitialized() => isInitialized;
}
