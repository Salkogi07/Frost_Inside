// GameNetworkManager.cs
// 역할: Steamworks API와 Netcode를 연결하여 로비 생성, 참가, 검색 등 핵심 기능을 수행합니다.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
using UnityEngine.SceneManagement;

public class GameNetworkManager : MonoBehaviour
{
    public static GameNetworkManager instance;
    
    public Lobby? CurrentLobby { get; private set; }
    public static string DisconnectReason { get; private set; } = "";
    
    private readonly List<ulong> bannedSteamIds = new List<ulong>();
    private bool isDisconnecting = false; // 중복 실행 방지를 위한 플래그
    
    [Header("Settings")]
    [SerializeField] private int maxPlayers = 4;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            for (int i = 0; i < bannedSteamIds.Count; i++)
            {
                Debug.Log(i+". "+bannedSteamIds[i]);
            }
            Debug.Log(DisconnectReason);
        }
    }

    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        
        // NetworkManager가 존재할 때만 콜백을 등록합니다.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    
    public void AddBannedPlayer(ulong steamId)
    {
        if (!bannedSteamIds.Contains(steamId))
        {
            bannedSteamIds.Add(steamId);
        }
    }

    public async void CreateLobby(string lobbyName, LobbyType lobbyType)
    {
        // 새로운 연결을 시작하므로, Disconnecting 플래그를 리셋합니다.
        isDisconnecting = false; 
        try
        {
            Debug.Log("Creating lobby...");
            var currentLobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);

            if (!currentLobby.HasValue)
            {
                Debug.LogError("Lobby creation failed.");
                return;
            }

            Lobby lobby = currentLobby.Value;
            lobby.SetData("Name", lobbyName);
            
            switch (lobbyType)
            {
                case LobbyType.Private:
                    lobby.SetPrivate();
                    break;
                case LobbyType.FriendsOnly:
                    lobby.SetFriendsOnly();
                    break;
                case LobbyType.Public:
                    lobby.SetPublic();
                    break;
            }
            
            lobby.SetJoinable(true);

            Debug.Log($"Lobby created! ID: {lobby.Id}, Name: {lobbyName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"An exception occurred while creating lobby: {e.Message}");
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        // 새로운 연결을 시작하므로, Disconnecting 플래그를 리셋합니다.
        isDisconnecting = false;
        if (await lobby.Join() != RoomEnter.Success)
        {
            Debug.LogError($"Failed to join lobby {lobby.Id}");
        }
    }

    public void JoinLobbyWithID(ulong lobbyId)
    {
        // 새로운 연결을 시작하므로, Disconnecting 플래그를 리셋합니다.
        isDisconnecting = false;
        SteamMatchmaking.JoinLobbyAsync(lobbyId);
    }
    
    public void LeaveLobbyIntentional()
    {
        SetDisconnectReason(""); // 이유를 비워서 메시지가 표시되지 않도록 함
        Disconnect();
    }
    
    public void Disconnect()
    {
        // Disconnect가 이미 진행 중이면 중복 실행 방지
        if (isDisconnecting) return;
        isDisconnecting = true;
        
        CurrentLobby?.Leave();
        CurrentLobby = null;
        
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost))
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        PlayerDataManager.instance?.ClearAllData();
        
        // isDisconnecting 플래그는 새로운 씬의 스크립트에서 리셋하거나
        // 새로운 연결 시도(Create/Join) 시 리셋합니다.
        SceneManager.LoadScene("Menu");
    }
    
    public static void SetDisconnectReason(string reason)
    {
        DisconnectReason = reason;
    }
    
    // 이 메소드는 그대로 유지됩니다.
    public void LockLobby()
    {
        if (NetworkManager.Singleton.IsHost && CurrentLobby.HasValue)
        {
            CurrentLobby.Value.SetJoinable(false);
            Debug.Log("Lobby has been locked. No new players can join.");
        }
    }
    
    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            NetworkManager.Singleton.StartHost();
        }
        else 
        {
            SetDisconnectReason("Failed to create lobby.");
            Disconnect();
        }
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        CurrentLobby = lobby;

        if (NetworkManager.Singleton.IsHost)
        {
            // [수정됨] 호스트는 즉시 "LobbyGame" 씬으로 전환을 시작합니다.
            // 이 함수는 서버와 모든 클라이언트의 씬을 동기화하여 로드합니다.
            NetworkManager.Singleton.SceneManager.LoadScene("LobbyGame", LoadSceneMode.Single);
        }
        else
        {
            // [수정됨] 클라이언트는 서버에 연결하기만 하면 됩니다.
            // 서버가 씬 전환을 관리하므로 클라이언트 측에서 씬을 로드할 필요가 없습니다.
            NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
            NetworkManager.Singleton.StartClient();
        }
    }
    
    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        JoinLobby(lobby);
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            PlayerDataManager.instance.UpdatePlayerCharacter(clientId, 0); 
            NetworkTransmission.instance.AnnounceMyselfToServerRpc(SteamClient.SteamId, SteamClient.Name);
        }
    }
    
    /*private IEnumerator LoadLobbySceneAndNotify()
    {
        var loadOperation = SceneManager.LoadSceneAsync("Lobby");
        while (!loadOperation.isDone)
        {
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        ChatManager.instance?.AddMessage("You have joined the lobby.", MessageType.PersonalSystem);
    }*/

    private void OnClientDisconnected(ulong clientId)
    {
        // 이미 Disconnect 절차가 시작되었다면 아무것도 하지 않음
        if (isDisconnecting) return;

        var playerInfo = PlayerDataManager.instance.GetPlayerInfo(clientId);

        if (NetworkManager.Singleton.IsHost) // 호스트: 떠난 클라이언트를 모두에게 알림
        {
            if (playerInfo != null)
                NetworkTransmission.instance.RemovePlayerClientRpc(clientId, playerInfo.SteamName);
        }
        else // 클라이언트: 비정상적으로 연결이 끊김
        {
            // 클라이언트가 이 콜백을 받았다는 것은 연결이 강제로 끊겼다는 의미입니다.
            // (추방, 호스트 이탈, 네트워크 문제 등)
            // 이미 DisconnectReason이 설정되어 있지 않다면 (예: 추방 메시지),
            // 일반적인 연결 끊김 메시지를 설정합니다.
            if (string.IsNullOrEmpty(DisconnectReason))
            {
                SetDisconnectReason("Connection to the host was lost.");
            }
            
            // 연결 종료 절차를 시작합니다.
            Disconnect();
        }
    }
    
    private void OnLobbyMemberLeave(Lobby lobby, Friend friend) { /* Netcode's OnClientDisconnected handles this */ }
}