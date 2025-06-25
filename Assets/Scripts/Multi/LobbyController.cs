// --- START OF FILE LobbyController.cs ---

using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using System.Linq;
using TMPro;

/// <summary>
/// 로비 씬의 UI와 로직을 제어하는 클래스입니다.
/// </summary>
public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private TextMeshProUGUI readyButtonText;

    [Header("Player List")]
    [SerializeField] private GameObject playerListViewContent;
    [SerializeField] private GameObject playerListItemPrefab;

    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    private bool isReady = false;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 버튼 이벤트 리스너 등록
        startGameButton.onClick.AddListener(OnClick_StartGame);
        readyButton.onClick.AddListener(OnClick_ReadyPlayer);
        leaveButton.onClick.AddListener(OnClick_LeaveLobby);
    }
    
    #endregion

    #region UI Callbacks

    /// <summary>
    /// 게임 시작 버튼 클릭 이벤트 핸들러입니다. 방장만 호출할 수 있습니다.
    /// </summary>
    private void OnClick_StartGame()
    {
        SteamManager.Instance.StartGameServer();
    }

    /// <summary>
    /// 로비 나가기 버튼 클릭 이벤트 핸들러입니다.
    /// </summary>
    public void OnClick_LeaveLobby()
    {
        // SteamManager와 CurrentLobby가 유효한지 안전하게 확인
        if (SteamManager.Instance != null && SteamManager.Instance.CurrentLobby.HasValue)
        {
            var lobby = SteamManager.Instance.CurrentLobby.Value;
            bool isOwner = lobby.Owner.Id == SteamClient.SteamId;

            // 방장이 나가면 로비를 파괴하고, 일반 멤버는 그냥 나갑니다.
            if (isOwner)
            {
                Debug.Log("방장이 로비 파괴를 시도합니다.");
                SteamManager.Instance.DestroyLobby();
            }
            else
            {
                Debug.Log("일반 멤버가 로비를 나갑니다.");
                SteamManager.Instance.LeaveLobby();
            }
        }
    }

    /// <summary>
    /// 준비/준비해제 버튼 클릭 이벤트 핸들러입니다.
    /// </summary>
    public void OnClick_ReadyPlayer()
    {
        isReady = !isReady;
        // 플레이어의 'ready' 상태를 Steam 로비 멤버 데이터에 저장합니다.
        SteamManager.Instance.SetPlayerData("ready", isReady.ToString().ToLower());
        UpdateReadyButtonText();
    }

    #endregion
    
    #region Public Methods

    /// <summary>
    /// 모든 플레이어가 준비되었는지 확인하고 시작 버튼 상태를 업데이트합니다.
    /// </summary>
    public void CheckIfAllReady()
    {
        var lobby = SteamManager.Instance.CurrentLobby;
        if (!lobby.HasValue)
        {
            if(startGameButton != null) startGameButton.gameObject.SetActive(false);
            return;
        }
        
        bool isOwner = lobby.Value.Owner.Id == SteamClient.SteamId;
        
        // 시작 버튼은 방장에게만 보입니다.
        if(startGameButton != null)
        {
            startGameButton.gameObject.SetActive(isOwner);
        }

        // 방장이 아니면 더 이상 진행할 필요가 없습니다.
        if (!isOwner) return;

        // --- 아래 코드는 방장인 경우에만 실행됩니다 ---

        // 모든 멤버가 준비되었는지 확인
        bool allReady = lobby.Value.Members.All(member => lobby.Value.GetMemberData(member, "ready") == "true");

        // 모든 멤버가 준비되었을 때만 시작 버튼을 활성화(interactable) 합니다.
        if(startGameButton != null)
        {
            startGameButton.interactable = allReady;
        }
    }
    
    /// <summary>
    /// UI에 표시될 로비 이름을 업데이트합니다.
    /// </summary>
    public void UpdateLobbyName()
    {
        var lobby = SteamManager.Instance.CurrentLobby;
        if (lobby.HasValue)
        {
            // [개선] 로비 데이터에서 'name' 키 값을 가져와 텍스트를 설정합니다.
            lobbyNameText.text = lobby.Value.GetData("name");
        }
    }

    /// <summary>
    /// 플레이어 목록 UI를 최신 상태로 업데이트합니다.
    /// </summary>
    public void UpdatePlayerList()
    {
        var lobby = SteamManager.Instance.CurrentLobby;
        if (!lobby.HasValue) return;

        var members = lobby.Value.Members.ToList();
        
        // 현재 목록에 있지만 더 이상 로비에 없는 플레이어 아이템 제거
        var itemsToRemove = playerListItems.Where(item => !members.Any(member => member.Id == item.PlayerSteamID)).ToList();
        foreach (var item in itemsToRemove)
        {
            Destroy(item.gameObject);
            playerListItems.Remove(item);
        }

        // 로비 멤버 목록을 순회하며 UI 아이템을 추가/업데이트
        foreach (var member in members)
        {
            var existingItem = playerListItems.FirstOrDefault(item => item.PlayerSteamID == member.Id);
            if (existingItem == null)
            {
                // 새 플레이어 아이템 생성
                GameObject newPlayerItem = Instantiate(playerListItemPrefab, playerListViewContent.transform);
                PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();
                playerListItems.Add(newPlayerItemScript);
                existingItem = newPlayerItemScript;
            }

            // 플레이어 정보 업데이트
            existingItem.PlayerName = member.Name;
            existingItem.PlayerSteamID = member.Id;
            existingItem.Ready = lobby.Value.GetMemberData(member, "ready") == "true";
            existingItem.IsLobbyOwner = (member.Id == lobby.Value.Owner.Id);

            // UI 값 설정
            existingItem.SetPlayerValues();
        }
        
        // 플레이어 목록이 변경되었으므로, 시작 가능 상태를 다시 확인합니다.
        CheckIfAllReady();
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// 준비 버튼의 텍스트를 현재 상태에 맞게 변경합니다.
    /// </summary>
    private void UpdateReadyButtonText()
    {
        readyButtonText.text = isReady ? "Ready" : "Unready";
    }

    #endregion
}