// --- START OF FILE HostLobbyController.cs ---

using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

/// <summary>
/// 로비 생성 UI 및 관련 로직을 관리하는 클래스입니다.
/// </summary>
public class HostLobbyController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject hostLobbyPanel;
    [SerializeField] private GameObject mainMenuButtonsPanel;
    
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Toggle friendsOnlyToggle;
    
    [Header("Buttons")]
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button backButton;
    
    private void Start()
    {
        createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    /// <summary>
    /// 로비 생성 패널을 활성화합니다.
    /// </summary>
    public void ShowHostLobbyPanel()
    {
        mainMenuButtonsPanel.SetActive(false);
        hostLobbyPanel.SetActive(true);
        
        // [개선] 기본 로비 이름을 현재 스팀 유저의 이름으로 설정합니다.
        lobbyNameInput.text = $"{SteamClient.Name}'s Lobby";
    }

    /// <summary>
    /// '로비 생성' 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnCreateLobbyClicked()
    {
        if (string.IsNullOrWhiteSpace(lobbyNameInput.text))
        {
            Debug.LogWarning("Lobby name cannot be empty.");
            return;
        }
        
        LobbyType lobbyType = friendsOnlyToggle.isOn ? LobbyType.FriendsOnly : LobbyType.Public;
        
        SteamManager.Instance.HostLobby(lobbyType, lobbyNameInput.text);
        
        // 로비 생성을 요청한 후, 패널을 닫습니다.
        // 실제 로비 입장은 OnLobbyEntered 콜백에서 처리됩니다.
        hostLobbyPanel.SetActive(false);
    }

    /// <summary>
    /// '뒤로가기' 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnBackButtonClicked()
    {
        hostLobbyPanel.SetActive(false);
        mainMenuButtonsPanel.SetActive(true);
    }
}