using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkTransmission : NetworkBehaviour
{
    public static NetworkTransmission instance;
    
    private bool isGameSetupInProgress = false;

    private void Awake()
    {
        if(instance != null)
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
            SyncExistingPlayerToNewClientRpc(player.ClientId, player.SteamId, player.SteamName, player.IsReady, player.SelectedCharacterId, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { newClientId } } });
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
    private void SyncExistingPlayerToNewClientRpc(ulong clientId, ulong steamId, string steamName, bool isReady, int charId, ClientRpcParams clientRpcParams = default)
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
            Debug.LogWarning($"Non-host client {rpcParams.Receive.SenderClientId} tried to kick a player. Request ignored.");
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
            ChatManager.instance?.AddMessage($"{kickedPlayerName} has been {reasonMessage} by the host.", MessageType.AdminSystem);
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
    
    // --- Game Flow ---
    [ServerRpc(RequireOwnership = false)]
    public void RequestStartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        if (PlayerDataManager.instance.AreAllPlayersReady())
        {
            GameNetworkManager.instance.LockLobby();
            PlayerDataManager.instance.ClearLoadedClients();

            // 모든 클라이언트에게 씬 로드 전에 로딩 화면을 먼저 켜도록 명령
            ShowLoadingScreenClientRpc();

            // LoadingManager를 거치지 않고 서버가 직접 씬 로드를 시작
            LoadingManager.instance.AnimLate("Shader_Out", "Game");
        }
        else
        {
            Debug.LogWarning($"Client {rpcParams.Receive.SenderClientId} requested game start, but not all players are ready.");
        }
    }
    
    [ClientRpc]
    private void ShowLoadingScreenClientRpc()
    {
        LoadingManager.instance.ShowLoadingScreen();
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
        Debug.Log("[Server] 맵 생성 및 초기 설정을 시작합니다.");
        
        // 맵 생성 로직
        yield return null;

        // GameManager 인스턴스를 찾아 게임 타이머 시작을 명령합니다.
        if (GameManager.instance == null)
        {
            Debug.LogError("[NetworkTransmission] GameManager instance not found! The game timer will not start.");
            yield break;
        }
        
        Debug.Log("[Server] 모든 플레이어 스폰을 명령합니다.");
        GameManager.instance.gamePlayerSpawner.SpawnAllPlayersForGame();

        GameManager.instance.StartGameTimer();


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
}