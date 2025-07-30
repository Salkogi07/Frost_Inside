using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System.Threading.Tasks;
using System.Collections.Generic;
using Steamworks.Data;

public class LobbyListFriendUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbyListPanel;
    [SerializeField] private Transform lobbyListContent;
    [SerializeField] private GameObject lobbyListItemPrefab;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button backButton;

    private List<GameObject> currentLobbyItems = new List<GameObject>();

    private void OnEnable()
    {
        RefreshLobbyList();
    }

    private void Start()
    {
        refreshButton.onClick.AddListener(RefreshLobbyList);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnBackButtonClicked()
    {
        lobbyListPanel.SetActive(false);
    }

    public async void RefreshLobbyList()
    {
        // 기존 목록 삭제
        foreach (var item in currentLobbyItems)
        {
            Destroy(item);
        }
        currentLobbyItems.Clear();
        
        foreach (var friend in SteamFriends.GetFriends()) {
            // 현재 같은 게임을 플레이 중인지 확인
            if (friend.IsPlayingThisGame && friend.GameInfo?.Lobby != null)
            {
                var lobbyId = friend.GameInfo?.Lobby?.Id ?? 0;
                Debug.Log($"친구 {friend.Name}의 로비 발견: ID = {lobbyId}");
                
                var newItem = Instantiate(lobbyListItemPrefab, lobbyListContent);
                newItem.GetComponent<LobbyFriendDataUI>().Setup(lobbyId, friend.Id, friend.Name);
                currentLobbyItems.Add(newItem);
            }
        }
    }
}