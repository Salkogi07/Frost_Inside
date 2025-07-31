// LobbyUI.cs
// 역할: 로비 씬의 전반적인 UI를 관리합니다. PlayerDataManager의 이벤트를 받아 UI를 갱신합니다.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerItemPrefab;
    [SerializeField] private Toggle allReadyToggle;
    
    // TODO: 여기에 실제 캐릭터 선택 패널/버튼 등을 연결
    // public CharacterSelectionPanel characterSelectionPanel;

    private Dictionary<ulong, PlayerDataUI> playerItemUIs = new Dictionary<ulong, PlayerDataUI>();

    private void OnEnable()
    {
        var lobby = GameNetworkManager.instance.CurrentLobby;
        if (lobby.HasValue)
        {
            lobbyNameText.text = lobby.Value.GetData("Name");
        }
        
        readyButton.onClick.AddListener(() => SetReady(!PlayerDataManager.instance.MyInfo.IsReady));
        startGameButton.onClick.AddListener(StartGame);
        leaveButton.onClick.AddListener(LeaveLobby);

        PlayerDataManager.instance.OnPlayerAdded += AddPlayerUI;
        PlayerDataManager.instance.OnPlayerRemoved += RemovePlayerUI;
        PlayerDataManager.instance.OnPlayerUpdated += UpdatePlayerUI;

        // 로비에 이미 있는 플레이어들의 UI를 생성
        foreach (var player in PlayerDataManager.instance.GetAllPlayers())
        {
            AddPlayerUI(player);
        }
        UpdateLobbyControls();
    }

    private void OnDisable()
    {
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.OnPlayerAdded -= AddPlayerUI;
            PlayerDataManager.instance.OnPlayerRemoved -= RemovePlayerUI;
            PlayerDataManager.instance.OnPlayerUpdated -= UpdatePlayerUI;
        }
    }
    
    private void AddPlayerUI(PlayerInfo info)
    {
        if (playerItemUIs.ContainsKey(info.ClientId)) return;
        var itemGO = Instantiate(playerItemPrefab, playerListContent);
        var itemUI = itemGO.GetComponent<PlayerDataUI>();
        
        bool isHostPlayer = info.ClientId == NetworkManager.ServerClientId;
        itemUI.Setup(info, isHostPlayer);
        
        playerItemUIs[info.ClientId] = itemUI;
        UpdateLobbyControls();
    }

    private void RemovePlayerUI(ulong clientId)
    {
        if (playerItemUIs.TryGetValue(clientId, out var itemUI))
        {
            Destroy(itemUI.gameObject);
            playerItemUIs.Remove(clientId);
        }
        UpdateLobbyControls();
    }

    private void UpdatePlayerUI(PlayerInfo info)
    {
        if (playerItemUIs.TryGetValue(info.ClientId, out var itemUI))
        {
            bool isHostPlayer = info.ClientId == NetworkManager.ServerClientId;
            itemUI.UpdateUI(info, isHostPlayer);
        }
        UpdateLobbyControls();
    }
    
    private void UpdateLobbyControls()
    {
        bool isHost = NetworkManager.Singleton.IsHost;
        startGameButton.gameObject.SetActive(isHost);
        
        if (isHost)
        {
            startGameButton.interactable = PlayerDataManager.instance.AreAllPlayersReady();
        }
        allReadyToggle.isOn = PlayerDataManager.instance.AreAllPlayersReady();
    }
    
    private void SetReady(bool isReady)
    {
        if (PlayerDataManager.instance.MyInfo.SelectedCharacterId == -1)
        {
            ChatManager.instance.AddMessage("Please select a character before getting ready!", MessageType.PersonalSystem);
            return;
        }
        NetworkTransmission.instance.SetMyReadyStateServerRpc(isReady);
    }
    
    // 캐릭터 선택 UI에서 이 함수를 호출하게 하세요.
    public void SelectCharacter(int characterId)
    {
        if(PlayerDataManager.instance.MyInfo.SelectedCharacterId == characterId)
            return;
        
        ChatManager.instance.AddMessage($"Character {characterId} has been selected.", MessageType.PersonalSystem);
        NetworkTransmission.instance.SetMyCharacterServerRpc(characterId);
    }

    private void StartGame() => NetworkTransmission.instance.RequestStartGameServerRpc();
    private void LeaveLobby() => GameNetworkManager.instance.Disconnect();
}