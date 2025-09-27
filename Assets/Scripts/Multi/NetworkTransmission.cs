using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class NetworkTransmission : NetworkBehaviour
{
    public static NetworkTransmission instance;

    private bool isGameSetupInProgress = false;

    private PlayerResultInfo[] _finalResults;
    private readonly HashSet<ulong> clientsReadyForLobby = new HashSet<ulong>();
    private Coroutine returnToLobbyCoroutine;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    // --- Player Connection ---
    [ServerRpc(RequireOwnership = false)]
    public void AnnounceMyselfToServerRpc(ulong steamId, string steamName, ServerRpcParams rpcParams = default)
    {
        ulong newClientId = rpcParams.Receive.SenderClientId;

        // 새로운 클라이언트에게 기존 플레이어 목록 전송
        foreach (var player in PlayerDataManager.instance.GetAllPlayers())
        {
            SyncExistingPlayerToNewClientRpc(player.ClientId, player.SteamId, player.SteamName, player.IsReady,
                player.SelectedCharacterId,
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { newClientId } } });
        }

        // 모든 클라이언트에게 새로운 플레이어 정보 전송
        SyncNewPlayerToAllClientRpc(newClientId, steamId, steamName);
    }

    [ClientRpc]
    private void SyncNewPlayerToAllClientRpc(ulong newClientId, ulong steamId, string steamName)
    {
        PlayerDataManager.instance.AddPlayer(newClientId, steamId, steamName);
        if (newClientId != NetworkManager.Singleton.LocalClientId)
        {
            ChatManager.instance?.AddMessage($"{steamName} has joined.", MessageType.GlobalSystem);
        }
    }

    [ClientRpc]
    private void SyncExistingPlayerToNewClientRpc(ulong clientId, ulong steamId, string steamName, bool isReady,
        int charId, ClientRpcParams clientRpcParams = default)
    {
        PlayerDataManager.instance.AddPlayer(clientId, steamId, steamName);
        PlayerDataManager.instance.UpdatePlayerReadyStatus(clientId, isReady);
        PlayerDataManager.instance.UpdatePlayerCharacter(clientId, charId);
    }

    [ClientRpc]
    public void RemovePlayerClientRpc(ulong clientId, string steamName)
    {
        if (PlayerDataManager.instance.GetPlayerInfo(clientId) != null)
        {
            PlayerDataManager.instance.RemovePlayer(clientId);
            ChatManager.instance?.AddMessage($"{steamName} has left.", MessageType.GlobalSystem);
        }
    }

    // --- Player Kick/Ban ---
    [ServerRpc(RequireOwnership = false)]
    public void RequestKickPlayerServerRpc(ulong targetClientId, bool shouldBan, ServerRpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            Debug.LogWarning(
                $"Non-host client {rpcParams.Receive.SenderClientId} tried to kick a player. Request ignored.");
            return;
        }

        var playerToKick = PlayerDataManager.instance.GetPlayerInfo(targetClientId);
        if (playerToKick == null) return;

        string kickedPlayerName = playerToKick.SteamName;

        if (shouldBan && GameNetworkManager.instance.CurrentLobby.HasValue)
        {
            Debug.Log($"Banning player {kickedPlayerName} (SteamID: {playerToKick.SteamId}) from lobby is requested.");
            GameNetworkManager.instance.AddBannedPlayer(playerToKick.SteamId);
        }

        // 모든 클라이언트에게 킥/밴 사실을 알립니다.
        NotifyPlayerKickedClientRpc(targetClientId, kickedPlayerName, shouldBan);
    }

    [ClientRpc]
    private void NotifyPlayerKickedClientRpc(ulong kickedClientId, string kickedPlayerName, bool wasBanned)
    {
        string reasonMessage = wasBanned ? "banned" : "kicked";

        // 만약 내가 킥당한 클라이언트라면
        if (kickedClientId == NetworkManager.Singleton.LocalClientId)
        {
            GameNetworkManager.SetDisconnectReason($"You have been {reasonMessage} by the host.");
            GameNetworkManager.instance.Disconnect();
        }
        else // 다른 클라이언트들에게는 알림 메시지만 표시
        {
            if (PlayerDataManager.instance.GetPlayerInfo(kickedClientId) != null)
            {
                PlayerDataManager.instance.RemovePlayer(kickedClientId);
            }

            // 채팅창에 알림
            ChatManager.instance?.AddMessage($"{kickedPlayerName} has been {reasonMessage} by the host.",
                MessageType.AdminSystem);
        }
    }

    // --- Chat ---
    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        ReceiveChatMessageClientRpc(message, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ReceiveChatMessageClientRpc(string message, ulong fromClientId)
    {
        string senderName = PlayerDataManager.instance.GetPlayerInfo(fromClientId)?.SteamName ?? "Unknown";

        ChatManager.instance?.AddMessage($"{senderName}: {message}", MessageType.PlayerMessage);
    }

    // --- Player State Sync ---
    [ServerRpc(RequireOwnership = false)]
    public void SetMyReadyStateServerRpc(bool isReady, ServerRpcParams rpcParams = default)
    {
        UpdatePlayerReadyStateClientRpc(rpcParams.Receive.SenderClientId, isReady);
    }

    [ClientRpc]
    private void UpdatePlayerReadyStateClientRpc(ulong clientId, bool isReady)
    {
        PlayerDataManager.instance.UpdatePlayerReadyStatus(clientId, isReady);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMyCharacterServerRpc(int characterId, ServerRpcParams rpcParams = default)
    {
        UpdatePlayerCharacterClientRpc(rpcParams.Receive.SenderClientId, characterId);
    }

    [ClientRpc]
    private void UpdatePlayerCharacterClientRpc(ulong clientId, int characterId)
    {
        PlayerDataManager.instance.UpdatePlayerCharacter(clientId, characterId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCharacterChangeServerRpc(int newCharacterId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        var playerInfo = PlayerDataManager.instance.GetPlayerInfo(clientId);

        if (playerInfo == null || playerInfo.SelectedCharacterId == newCharacterId)
        {
            return;
        }

        UpdatePlayerCharacterClientRpc(clientId, newCharacterId);

        // 이 부분이 핵심입니다. 서버가 PlayerSpawner를 통해 캐릭터 교체를 명령합니다.
        FindObjectOfType<PlayerSpawnController>().RespawnPlayerCharacter(clientId, newCharacterId);
    }
    
    /// <summary>
    /// (클라이언트 -> 서버) 클라이언트가 자신의 사망 상태를 서버에 보고합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ReportMyDeathServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        // 서버의 PlayerDataManager에 사망 상태를 기록하고 모든 클라이언트에게 전파
        UpdatePlayerDeadStatusClientRpc(clientId, true);
    }

    /// <summary>
    /// (서버 -> 클라이언트) 모든 클라이언트에게 특정 플레이어의 사망 상태를 업데이트하도록 알립니다.
    /// </summary>
    [ClientRpc]
    private void UpdatePlayerDeadStatusClientRpc(ulong clientId, bool isDead)
    {
        PlayerDataManager.instance.UpdatePlayerDeadStatus(clientId, isDead);
    }

    // --- Game Flow ---
    [ServerRpc(RequireOwnership = false)]
    public void RequestStartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        // 게임 시작 조건: 모든 플레이어 준비 + 미션 수락 완료
        if (PlayerDataManager.instance.AreAllPlayersReady() && MissionManager.instance.IsMissionAccepted)
        {
            GameNetworkManager.instance.LockLobby();
            PlayerDataManager.instance.ClearLoadedClients();

            ShowLoadingScreenClientRpc();

            LoadingManager.instance.AnimLate("Shader_Out", "Game");
        }
        else
        {
            if (!PlayerDataManager.instance.AreAllPlayersReady())
            {
                Debug.LogWarning(
                    $"Client {rpcParams.Receive.SenderClientId} requested game start, but not all players are ready.");
            }

            if (!MissionManager.instance.IsMissionAccepted)
            {
                Debug.LogWarning(
                    $"Client {rpcParams.Receive.SenderClientId} requested game start, but no mission was accepted.");
                // 요청한 클라이언트에게만 미션 미선택 메시지 전송
                NotifyMissionNotSelectedClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
                });
            }
        }
    }

    [ClientRpc]
    private void NotifyMissionNotSelectedClientRpc(ClientRpcParams clientRpcParams = default)
    {
        ChatManager.instance?.AddMessage("A mission has not been selected. Cannot start the game.",
            MessageType.PersonalSystem);
    }

    [ClientRpc]
    private void ShowLoadingScreenClientRpc()
    {
        LoadingManager.instance.ShowLoadingScreen();
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyServerMapGeneratedServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        PlayerDataManager.instance.AddMapGeneratedClient(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyServerSceneLoadedServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        PlayerDataManager.instance.AddLoadedClient(clientId);

        if (isGameSetupInProgress) return;

        if (PlayerDataManager.instance.GetLoadedClientCount() >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            isGameSetupInProgress = true;
            Debug.Log("[Server] All clients have loaded the scene. Starting server-side setup...");
            StartCoroutine(ServerGameSetupCoroutine());
        }
    }

    private IEnumerator ServerGameSetupCoroutine()
    {
        Debug.Log("[Server] 초기 설정을 시작합니다.");

        TimerManager timerManager = TimerManager.instance;
        // TimeManager 찾기
        if (timerManager == null)
        {
            Debug.LogError("[NetworkTransmission] TimeManager instance not found!");
            yield break;
        }

        GameManager gameManager = GameManager.instance;
        // GameManager 찾기
        if (gameManager == null)
        {
            Debug.LogError("[NetworkTransmission] GameManager instance not found!");
            yield break;
        }

        // 1. 카운터 초기화
        PlayerDataManager.instance.ClearMapGeneratedClients();
        PlayerDataManager.instance.ClearPlayerSpawnedClients();

        // 2. 맵 생성 명령 및 대기 (아이템 스폰은 맵 생성 마지막 단계에 서버에서 자동으로 실행됨)
        Debug.Log("[Server] 맵 생성을 시작합니다.");
        int seed = System.Environment.TickCount;
        GenerateMapClientRpc(seed);

        Debug.Log("[Server] 모든 클라이언트가 맵 생성을 완료할 때까지 기다리는 중입니다...");
        yield return new WaitUntil(() =>
            PlayerDataManager.instance.GetMapGeneratedClientCount() >= NetworkManager.Singleton.ConnectedClients.Count
        );
        Debug.Log("[Server] 모든 클라이언트가 맵 생성을 확인했습니다. 아이템 스폰이 완료되었습니다.");

        // 3. 플레이어 스폰 명령 및 대기
        Debug.Log("[Server] 모든 플레이어 스폰을 명령합니다.");
        gameManager.gamePlayerSpawner.SpawnAllPlayersForGame();

        Debug.Log("[Server] 모든 플레이어가 스폰될 때까지 기다리는 중입니다...");
        yield return new WaitUntil(() =>
            PlayerDataManager.instance.GetPlayerSpawnedClientCount() >= NetworkManager.Singleton.ConnectedClients.Count
        );
        Debug.Log("[Server] 모든 플레이어가 스폰되었습니다.");

        // 4. 게임 타이머 시작
        Debug.Log("[Server] 게임 타이머를 시작합니다.");
        timerManager.StartGameTimer();

        // 5. 몬스터 스포너 초기화 및 시작
        Debug.Log("[Server] 몬스터 스포너를 초기화하고 시작합니다.");
        MakeRandomMap mapComponent = gameManager.makeRandomMap;
        if (mapComponent != null && gameManager.monsterSpawner != null)
        {
            gameManager.monsterSpawner.InitializeAndStartSpawning(mapComponent, mapComponent.spreadTilemap);
        }
        else
        {
            Debug.LogError("[Server] MakeRandomMap 또는 MonsterSpawner를 찾을 수 없어 몬스터 스폰을 시작할 수 없습니다!");
        }

        Debug.Log("[Server] Server-side setup is complete. Notifying clients to start the game.");

        // 모든 준비가 끝났으므로, 이제 클라이언트들에게 로딩 화면을 끄라고 명령합니다.
        AllSetupCompleteStartGameClientRpc();
    }


    [ClientRpc]
    private void AllSetupCompleteStartGameClientRpc()
    {
        Debug.Log("[Client] Received 'All Setup Complete' signal from server. Hiding loading screen.");
        LoadingManager.instance.HideLoadingScreen();
    }

    [ClientRpc]
    public void GenerateMapClientRpc(int seed)
    {
        if (GameManager.instance != null && GameManager.instance.makeRandomMap != null)
        {
            Debug.Log($"[Client] Received command to generate map with seed: {seed}");
            GameManager.instance.makeRandomMap.GenerateMapFromSeed(seed);
        }
        else
        {
            Debug.LogError("[Client] Could not find GameManager or MakeRandomMap to generate map.");
        }
    }

    /// <summary>
    /// (서버 전용) CruiserController가 생성한 최종 결산 정보를 저장합니다.
    /// </summary>
    public void SetFinalPlayerResults(PlayerResultInfo[] results)
    {
        if (!IsServer) return;
        _finalResults = results;
    }

    /// <summary>
    /// (서버 전용) 게임을 종료하고 모든 클라이언트에게 결산 화면을 표시하도록 명령합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void EndGameAndShowResultsServerRpc()
    {
        if (!IsServer) return;

        Debug.Log("[Server] Ending game and showing results.");
        ShowResultsClientRpc(_finalResults);

        // 30초 후 자동 로비 복귀 타이머 시작
        if (returnToLobbyCoroutine != null) StopCoroutine(returnToLobbyCoroutine);
        returnToLobbyCoroutine = StartCoroutine(ReturnToLobbyTimer());
    }

    [ClientRpc]
    private void ShowResultsClientRpc(PlayerResultInfo[] results)
    {
        Debug.Log("[Client] Received results. Showing result screen.");
        ResultsScreenUIManager.instance.Show(results);
    }

    /// <summary>
    /// 클라이언트가 결산 화면의 '다음' 버튼을 누르면 서버에 알립니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void NotifyReadyForNextSceneServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        clientsReadyForLobby.Add(clientId);

        // 현재 연결된 모든 클라이언트가 준비되었는지 확인
        if (clientsReadyForLobby.Count >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            if (returnToLobbyCoroutine != null) StopCoroutine(returnToLobbyCoroutine);

            Debug.Log("[Server] All clients are ready. Returning to lobby.");
            ReturnToLobby();
        }
    }

    private IEnumerator ReturnToLobbyTimer()
    {
        yield return new WaitForSeconds(30f);
        Debug.Log("[Server] 30-second timer elapsed. Force returning to lobby.");
        ReturnToLobby();
    }

    /// <summary>
    /// (서버 전용) 모든 클라이언트를 로비 씬으로 이동시킵니다.
    /// </summary>
    private void ReturnToLobby()
    {
        if (!IsServer) return;

        clientsReadyForLobby.Clear();
        // 호스트가 NetworkSceneManager를 사용해 모든 클라이언트의 씬을 전환합니다.
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyGame", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}