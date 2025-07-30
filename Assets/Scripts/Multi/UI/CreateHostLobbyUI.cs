// CreateHostLobbyUI.cs

using Steamworks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateHostLobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject createLobbyPanel;
    
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Toggle friendsOnlyToggle;
    [SerializeField] private Button createButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        lobbyNameInput.text = $"{SteamClient.Name}'s Lobby";
        
        createButton.onClick.AddListener(CreateLobby);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }
    
    public void OnBackButtonClicked()
    {
        createLobbyPanel.SetActive(false);
    }

    private void CreateLobby()
    {
        if (string.IsNullOrWhiteSpace(lobbyNameInput.text))
        {
            Debug.LogWarning("Lobby name cannot be empty.");
            return;
        }
        
        string lobbyName = lobbyNameInput.text;
        
        LobbyType lobbyType = friendsOnlyToggle.isOn ? LobbyType.FriendsOnly : LobbyType.Public;
        
        
        GameNetworkManager.instance.CreateLobby(lobbyName, lobbyType);
    }
}