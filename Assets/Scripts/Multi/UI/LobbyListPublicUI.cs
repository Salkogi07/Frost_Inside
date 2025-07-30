// LobbyListUI.cs
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;

public class LobbyListPublicUI : MonoBehaviour
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

    public void OnBackButtonClicked()
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

        // Steam에서 로비 목록 요청
        var lobbies = await SteamMatchmaking.LobbyList
            .WithSlotsAvailable(1)
            .RequestAsync();
            
        if (lobbies != null)
        {
            foreach (var lobby in lobbies)
            {
                Debug.Log(lobby);
                GameObject newItem = Instantiate(lobbyListItemPrefab, lobbyListContent);
                newItem.GetComponent<LobbyDataUI>().Setup(lobby);
                currentLobbyItems.Add(newItem);
            }
        }
    }
}