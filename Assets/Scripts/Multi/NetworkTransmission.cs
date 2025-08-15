// NetworkTransmission.cs
// 역할: 클라이언트와 서버 간의 모든 RPC 통신을 중앙에서 처리합니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class NetworkTransmission : NetworkBehaviour
{
    public static NetworkTransmission instance;

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
    
    // --- Game Start ---
    [ServerRpc(RequireOwnership = false)]
    public void RequestStartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        // 이 RPC는 호스트만 호출할 수 있도록 UI에서 제어해야 합니다.
        if (!IsServer) return;

        if (PlayerDataManager.instance.AreAllPlayersReady())
        {
            GameNetworkManager.instance.LockLobby();
            
            // Netcode의 SceneManager를 사용하여 씬 로딩을 시작합니다.
            // 이 함수를 호출하면 서버가 모든 클라이언트에게 "Game" 씬을 로드하라고 자동으로 명령합니다.
            // Netcode는 모든 클라이언트가 로딩을 마칠 때까지 기다렸다가 씬을 동시에 활성화합니다.
            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            // 만약의 경우를 대비한 로그 (예: 클라이언트가 RPC를 잘못 호출)
            ulong senderId = rpcParams.Receive.SenderClientId;
            Debug.LogWarning($"Client {senderId} requested game start, but not all players are ready.");
        }
    }
}