using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectScreen : MonoBehaviour
{
    [SerializeField]
    Canvas canvas;

    [SerializeField]
    Button buttonQuit;

    private void Awake()
    {
        canvas.enabled = false;
    }

    private void OnEnable()
    {
        //Subscribe to events
        StartCoroutine(ListenToNetworkManager());
        buttonQuit.onClick.AddListener(QuitSession);
    }

    private void OnDisable()
    {
        //Unsubscribe from events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
        buttonQuit.onClick.RemoveListener(QuitSession);
    }

    private void QuitSession() => NetworkManager.Singleton.Shutdown();

    private void HandleClientDisconnect(ulong clientID)
    {
        if (NetworkManager.Singleton.LocalClientId != clientID) return;

        canvas.enabled = false;
    }

    private void HandleClientConnected(ulong clientID)
    {
        if (NetworkManager.Singleton.LocalClientId != clientID) return;

        canvas.enabled = true;
    }
    IEnumerator ListenToNetworkManager()
    {
        //Wait for network manager to load
        while (NetworkManager.Singleton == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        //Subscribe to network manager events
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }
}
