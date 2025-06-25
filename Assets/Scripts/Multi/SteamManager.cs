// --- START OF FILE SteamManager.cs ---

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Linq;
using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public enum LobbyType
{
    FriendsOnly,
    Public,
    Private
}

/// <summary>
/// Steamworks API와의 모든 상호작용을 관리하는 싱글턴 클래스입니다.
/// </summary>
public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance;

    [Header("Settings")]
    [SerializeField] private int maxPlayers = 4;

    public Lobby? CurrentLobby { get; private set; }

    #region Unity Lifecycle & Singleton

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Steamworks 콜백 이벤트 구독
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
    }

    private void OnDisable()
    {
        // Steamworks 콜백 이벤트 구독 해제
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
    }
    
    #endregion

    #region Lobby Management
    
    /// <summary>
    /// 새로운 스팀 로비를 생성하고 호스트가 됩니다.
    /// </summary>
    public async void HostLobby(LobbyType lobbyType, string lobbyName)
    {
        try
        {
            Debug.Log("Creating lobby...");
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);

            if (!createLobbyOutput.HasValue)
            {
                Debug.LogError("Lobby creation failed.");
                return;
            }

            Lobby lobby = createLobbyOutput.Value;
            lobby.SetData("name", lobbyName);
            
            lobby.SetData("started", "false");
            lobby.SetData("closing", "false"); 

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

    /// <summary>
    /// 현재 로비를 파괴합니다. 방장만 호출할 수 있습니다.
    /// </summary>
    public void DestroyLobby()
    {
        if (CurrentLobby.HasValue && CurrentLobby.Value.Owner.Id == SteamClient.SteamId)
        {
            Debug.Log("Host is destroying the lobby.");
            CurrentLobby.Value.SetData("closing", "true");
            CurrentLobby.Value.Leave();
            CurrentLobby = null;
            if(NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost) NetworkManager.Singleton.Shutdown();
            TransitionToMainMenu();
        }
    }

    /// <summary>
    /// 현재 로비에서 나갑니다.
    /// </summary>
    public void LeaveLobby()
    {
        if (CurrentLobby.HasValue)
        {
            Debug.Log("Leaving the lobby.");
            CurrentLobby.Value.Leave();
            CurrentLobby = null;
            if(NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient) NetworkManager.Singleton.Shutdown();
            TransitionToMainMenu();
        }
    }
    
    /// <summary>
    /// ID를 사용하여 특정 로비에 참가합니다.
    /// </summary>
    public async void JoinLobby(SteamId lobbyId)
    {
        Lobby? lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
        if (!lobby.HasValue)
        {
            Debug.LogError($"Failed to join lobby {lobbyId}.");
        }
    }

    #endregion

    #region Lobby Search

    /// <summary>
    /// 공개 로비 목록을 가져옵니다.
    /// </summary>
    public async void GetPublicLobbies()
    {
        if (LobbyListManager.instance == null) return;
        LobbyListManager.instance.DestroyLobbyListItems();
        
        var lobbies = await SteamMatchmaking.LobbyList
            .WithSlotsAvailable(1)
            .WithKeyValue("started", "false")
            .RequestAsync();

        if (lobbies != null)
        {
            LobbyListManager.instance.DisplayLobbies(lobbies.ToList());
        }
    }

    /// <summary>
    /// 친구가 있는 로비 목록을 가져옵니다.
    /// </summary>
    public async void GetFriendLobbies()
    {
        if (LobbyListManager.instance == null) return;
        LobbyListManager.instance.DestroyLobbyListItems();
    
        List<Lobby> friendLobbies = new List<Lobby>();
        
        var friendsInLobbies = SteamFriends.GetFriends()
            .Where(friend => friend.IsPlayingThisGame && friend.GameInfo.HasValue && friend.GameInfo.Value.Lobby.HasValue);

        if (!friendsInLobbies.Any())
        {
            LobbyListManager.instance.DisplayLobbies(friendLobbies); 
            return;
        }
    
        foreach (var friend in friendsInLobbies)
        {
            Lobby lobby = friend.GameInfo.Value.Lobby.Value;
            lobby.Refresh(); 
            friendLobbies.Add(lobby);
        }

        await Task.Delay(200);

        var joinableFriendLobbies = friendLobbies.Where(l => l.GetData("started") != "true").ToList();

        LobbyListManager.instance.DisplayLobbies(joinableFriendLobbies);
    }

    #endregion
    
    #region Steam Callbacks
    
    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit 호출 — 로비에서 나갑니다.");

        if (CurrentLobby != null)
        {
            var lobby = CurrentLobby.Value;
            bool isOwner = lobby.Owner.Id == SteamClient.SteamId;

            // 방장이 나가면 로비를 파괴하고, 일반 멤버는 그냥 나갑니다.
            if (isOwner)
            {
                Debug.Log("방장이 로비 파괴를 시도합니다.");
                DestroyLobby();
            }
            else
            {
                Debug.Log("일반 멤버가 로비를 나갑니다.");
                LeaveLobby();
            }
        }
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            Debug.Log("OnLobbyCreated callback received. Starting host.");
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.LogError($"Lobby creation failed with result: {result}");
        }
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        CurrentLobby = lobby;
        Debug.Log($"Entered lobby: {lobby.Id}");
        
        SetPlayerData("ready", "false");
        
        SceneTransitionManager.Instance?.TransitionToScene("Lobby");
        
        if(!NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
            NetworkManager.Singleton.StartClient();
        }
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} joined the lobby.");
        LobbyController.Instance?.UpdatePlayerList();
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} left the lobby.");

        bool isLobbyClosed = lobby.GetData("closing") != "true";
        if (isLobbyClosed)
        {
            LobbyController.Instance?.UpdatePlayerList();
        }
    }
    
    private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} has disconnected from the lobby due to a network issue.");
        
        bool isLobbyClosed = lobby.GetData("closing") != "true";
        if (isLobbyClosed)
        {
            LobbyController.Instance?.UpdatePlayerList();
        }
    }

    private void OnLobbyDataChanged(Lobby lobby)
    {
        if (lobby.GetData("closing") == "true")
        {
            Debug.Log("Host has closed the lobby. Leaving.");
            LeaveLobby(); 
            return;
        }

        bool isCheckLobby = CurrentLobby.HasValue && CurrentLobby.Value.Id == lobby.Id;
        if (LobbyController.Instance != null && isCheckLobby)
        {
            LobbyController.Instance.UpdateLobbyName();
            LobbyController.Instance.UpdatePlayerList();
        }
    }
    
    #endregion
    
    #region Public Helpers
    
    public void SetPlayerData(string key, string value)
    {
        CurrentLobby?.SetMemberData(key, value);
    }

    public void StartGameServer()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if(CurrentLobby.HasValue)
            {
                CurrentLobby.Value.SetJoinable(false);
                CurrentLobby.Value.SetData("started", "true");
                Debug.Log("Lobby is now locked and game is starting.");
            }
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
    
    private void TransitionToMainMenu()
    {
        SceneTransitionManager.Instance?.TransitionToScene("MainMenu");
    }

    #endregion
}