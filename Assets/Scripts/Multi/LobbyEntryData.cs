// --- START OF FILE LobbyEntryData.cs ---

using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

/// <summary>
/// 로비 목록의 각 항목(Entry)에 대한 데이터와 동작을 정의하는 클래스입니다.
/// </summary>
public class LobbyEntryData : MonoBehaviour
{
    public SteamId lobbySteamID;
    public string lobbyName;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private Button joinLobbyButton;

    private void Start()
    {
        joinLobbyButton.onClick.AddListener(JoinLobby);
    }
    
    /// <summary>
    /// UI 텍스트에 로비 이름을 설정합니다.
    /// </summary>
    public void SetLobbyData()
    {
        lobbyNameText.text = string.IsNullOrEmpty(lobbyName) ? "Nameless Lobby" : lobbyName;
    }
  
    /// <summary>
    /// 이 로비에 참가합니다.
    /// </summary>
    public void JoinLobby()
    {
        SteamManager.Instance.JoinLobby(lobbySteamID);
    }
}