// --- START OF FILE ChatManager.cs ---
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;


/// <summary>
/// 로비 내 채팅 기능을 관리하는 클래스입니다.
/// </summary>
public class ChatManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private TextMeshProUGUI messageTemplate;
    [SerializeField] private GameObject messageContainer;
    [SerializeField] private ScrollRect chatScrollRect;
    
    [Header("Chat Settings")]
    [SerializeField] private UnityEngine.Color playerMessageColor = UnityEngine.Color.white;
    [SerializeField] private UnityEngine.Color systemMessageColor = UnityEngine.Color.yellow; // 시스템 메시지 색상 (예: 노란색)

    #region Unity Lifecycle
    
    private void Start()
    {
        // 메시지 템플릿은 복제용으로만 사용하므로 초기화 시 비워둡니다.
        messageTemplate.text = "";
        
        // SteamManager가 준비되었고, 로비에 입장한 상태라면 환영 메시지를 표시합니다.
        if (SteamManager.Instance != null && SteamManager.Instance.CurrentLobby.HasValue)
        {
            AddMessageToBox("You entered the lobby!", systemMessageColor);
        }
    }

    private void OnEnable()
    {
        // Steamworks 콜백 이벤트 구독
        SteamMatchmaking.OnChatMessage += OnChatMessageReceived;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
    }

    private void OnDisable()
    {
        // Steamworks 콜백 이벤트 구독 해제
        SteamMatchmaking.OnChatMessage -= OnChatMessageReceived;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
    }

    private void Update()
    {
        // 엔터 키 입력으로 채팅창 활성화/비활성화 및 메시지 전송
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleChatBox();
        }
    }

    #endregion

    #region Steam Callbacks
    
    private void OnLobbyMemberLeave(Lobby lobby, Friend friend) => AddMessageToBox($"{friend.Name} left the lobby.", systemMessageColor);
    private void OnLobbyMemberJoined(Lobby lobby, Friend friend) => AddMessageToBox($"{friend.Name} joined the lobby.", systemMessageColor);
    private void OnChatMessageReceived(Lobby lobby, Friend friend, string msg) => AddMessageToBox($"{friend.Name}: {msg}", playerMessageColor);
    
    private void OnLobbyEntered(Lobby lobby)
    {
        // Start()에서 이미 입장 메시지를 처리하므로 여기서는 특별한 동작 없음
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 채팅 UI에 새로운 메시지를 추가합니다.
    /// </summary>
    /// <param name="msg">표시할 메시지 문자열</param>
    /// <param name="color">메시지 텍스트 색상</param> // --- 파라미터 추가 ---
    private void AddMessageToBox(string msg, UnityEngine.Color color)
    {
        // 템플릿을 복제하여 새 메시지 오브젝트 생성
        GameObject messageObject = Instantiate(messageTemplate.gameObject, messageContainer.transform);
        
        var messageTextComponent = messageObject.GetComponent<TextMeshProUGUI>();
        messageTextComponent.text = msg;
        messageTextComponent.color = color; // 전달받은 색상으로 설정

        // 메시지 추가 후 스크롤을 맨 아래로 내림
        StartCoroutine(ScrollToBottomCoroutine());
    }

    /// <summary>
    /// UI 레이아웃 업데이트 후 스크롤 뷰를 맨 아래로 이동시키는 코루틴입니다.
    /// </summary>
    private IEnumerator ScrollToBottomCoroutine()
    {
        // UI 요소가 재배치될 때까지 현재 프레임의 끝에서 대기
        yield return new WaitForEndOfFrame();

        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// 채팅 입력창의 활성 상태를 토글하고 메시지를 전송합니다.
    /// </summary>
    private void ToggleChatBox()
    {
        if (messageInputField.gameObject.activeSelf)
        {
            // 입력창이 활성화된 상태에서 엔터를 누르면 메시지 전송
            if (!string.IsNullOrWhiteSpace(messageInputField.text))
            {
                SteamManager.Instance.CurrentLobby?.SendChatString(messageInputField.text);
                messageInputField.text = "";
            }
            
            // 입력창 비활성화 및 포커스 해제
            messageInputField.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            // 입력창이 비활성화된 상태에서 엔터를 누르면 활성화
            messageInputField.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(messageInputField.gameObject);
        }
    }

    #endregion
}