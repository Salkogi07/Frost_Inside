// LobbySetupUI.cs
using UnityEngine;
using UnityEngine.UI;

public class LobbySetupUI : MonoBehaviour
{
    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private GameObject lobbyListsFriendPanel;
    [SerializeField] private GameObject lobbyListsPublicPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button lobbyListsFriendButton;
    [SerializeField] private Button lobbyListsPublicButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        hostButton.onClick.AddListener(ShowCreateLobbyPanel);
        lobbyListsFriendButton.onClick.AddListener(ShowLobbyListsFriendPanel);
        lobbyListsPublicButton.onClick.AddListener(ShowLobbyListsPublicPanel);
        quitButton.onClick.AddListener(GameQuit);
    }
    
    public void ShowCreateLobbyPanel()
    {
        createLobbyPanel.SetActive(true);
        lobbyListsFriendPanel.SetActive(false);
        lobbyListsPublicPanel.SetActive(false);
    }

    public void ShowLobbyListsFriendPanel()
    {
        createLobbyPanel.SetActive(false);
        lobbyListsFriendPanel.SetActive(true);
        lobbyListsPublicPanel.SetActive(false);
    }
    
    public void ShowLobbyListsPublicPanel()
    {
        createLobbyPanel.SetActive(false);
        lobbyListsFriendPanel.SetActive(false);
        lobbyListsPublicPanel.SetActive(true);
    }

    public void GameQuit()
    {
        Application.Quit();
    }
}