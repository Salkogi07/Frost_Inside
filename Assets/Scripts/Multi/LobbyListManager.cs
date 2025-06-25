// --- START OF FILE LobbyListManager.cs ---

using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using UnityEngine.UI;

/// <summary>
/// 로비 목록 UI를 관리하는 클래스입니다.
/// </summary>
public class LobbyListManager : MonoBehaviour
{
    public static LobbyListManager instance;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject lobbyListMenu;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject lobbyEntryPrefab;
    [SerializeField] private GameObject scrollViewContent;
    
    [Header("Buttons")]
    [SerializeField] private Button backButton;

    private List<GameObject> listOfLobbiesUI = new List<GameObject>();
    
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        backButton.onClick.AddListener(OnClick_Back);
    }
    
    #region OnClick_Event_Function
    public void OnClick_GetFriendLobbies()
    {
        ShowLobbyListMenu();
        SteamManager.Instance.GetFriendLobbies();
    }
    
    public void OnClick_UpdateFriendLobbies()
    {
        ShowLobbyListMenu();
        SteamManager.Instance.GetFriendLobbies();
    }
    
    public void OnClick_GetPublicLobbies()
    {
        ShowLobbyListMenu();
        SteamManager.Instance.GetPublicLobbies();
    }
    
    public void OnClick_UpdatePublicLobbies()
    {
        ShowLobbyListMenu();
        SteamManager.Instance.GetPublicLobbies();
    }

    public void OnClick_Back()
    {
        lobbyListMenu.SetActive(false);
        mainMenu.SetActive(true);
        DestroyLobbyListItems();
    }
    #endregion

    /// <summary>
    /// 로비 목록을 받아와 UI에 표시합니다.
    /// </summary>
    /// <param name="lobbies">표시할 로비 목록</param>
    public void DisplayLobbies(List<Lobby> lobbies)
    {
        DestroyLobbyListItems();

        foreach (var lobby in lobbies)
        {
            string lobbyName = lobby.GetData("name");

            // [개선] 이름이 없는 로비는 목록에 표시하지 않습니다.
            if (string.IsNullOrEmpty(lobbyName))
                continue;
            
            GameObject createdLobbyItem = Instantiate(lobbyEntryPrefab, scrollViewContent.transform);
            LobbyEntryData lobbyData = createdLobbyItem.GetComponent<LobbyEntryData>();

            lobbyData.lobbySteamID = lobby.Id;
            lobbyData.lobbyName = $"{lobbyName} ({lobby.MemberCount}/{lobby.MaxMembers})"; // 인원수 표시
            lobbyData.SetLobbyData();

            listOfLobbiesUI.Add(createdLobbyItem);
        }
    }

    /// <summary>
    /// 현재 표시된 모든 로비 UI 아이템을 파괴합니다.
    /// </summary>
    public void DestroyLobbyListItems()
    {
        foreach (GameObject lobbyItem in listOfLobbiesUI)
        {
            Destroy(lobbyItem);
        }
        listOfLobbiesUI.Clear();
    }
    
    private void ShowLobbyListMenu()
    {
        mainMenu.SetActive(false);
        lobbyListMenu.SetActive(true);
    }
}