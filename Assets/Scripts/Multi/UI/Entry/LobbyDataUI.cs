// LobbyListItemUI.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Steamworks.Data;

public class LobbyDataUI : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton;

    private Lobby currentLobby;

    public void Setup(Lobby _lobby)
    {
        currentLobby = _lobby;
        lobbyNameText.text = _lobby.GetData("Name");
        playerCountText.text = $"{_lobby.MemberCount}/{_lobby.MaxMembers}";
        joinButton.onClick.AddListener(JoinLobby);
    }

    private void JoinLobby()
    {
        GameNetworkManager.instance.JoinLobby(currentLobby);
    }
}