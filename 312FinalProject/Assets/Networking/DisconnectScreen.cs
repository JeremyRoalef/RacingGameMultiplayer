using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectScreen : MonoBehaviour
{
    [SerializeField]
    Canvas canvas;

    [SerializeField]
    Button buttonQuit;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        canvas.enabled = false;
        buttonQuit.onClick.AddListener(QuitSession);
    }

    private void QuitSession()
    {
        NetworkManager.Singleton.Shutdown();
    }

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
}
