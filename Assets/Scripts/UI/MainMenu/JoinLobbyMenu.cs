using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameNetworkManager networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        GameNetworkManager.OnClientConnected += HandleClientConnected;
        GameNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        GameNetworkManager.OnClientConnected -= HandleClientConnected;
        GameNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;

        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
