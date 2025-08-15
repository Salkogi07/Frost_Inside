// PlayerDataManager.cs
// 역할: 로비의 모든 플레이어 데이터를 중앙에서 관리하고, 데이터 변경 시 이벤트를 발생시켜 UI와 로직을 분리합니다.

using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Linq;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;

    // Key: ClientId, Value: PlayerInfo
    private readonly Dictionary<ulong, PlayerInfo> playerInfoMap = new Dictionary<ulong, PlayerInfo>();

    public event Action<PlayerInfo> OnPlayerAdded;
    public event Action<ulong> OnPlayerRemoved;
    public event Action<PlayerInfo> OnPlayerUpdated;

    public PlayerInfo MyInfo => GetPlayerInfo(NetworkManager.Singleton.LocalClientId);

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

    public void AddPlayer(ulong clientId, ulong steamId, string steamName)
    {
        if (playerInfoMap.ContainsKey(clientId)) return;

        var newPlayer = new PlayerInfo
        {
            ClientId = clientId,
            SteamId = steamId,
            SteamName = steamName,
            IsReady = false,
            SelectedCharacterId = 0
        };
        playerInfoMap[clientId] = newPlayer;
        OnPlayerAdded?.Invoke(newPlayer);
        Debug.Log($"[PlayerDataManager] Player Added: {steamName} (Client: {clientId})");
    }

    public void RemovePlayer(ulong clientId)
    {
        if (!playerInfoMap.ContainsKey(clientId)) return;
        playerInfoMap.Remove(clientId);
        OnPlayerRemoved?.Invoke(clientId);
        Debug.Log($"[PlayerDataManager] Player Removed: (Client: {clientId})");
    }

    public void UpdatePlayerReadyStatus(ulong clientId, bool isReady)
    {
        if (playerInfoMap.TryGetValue(clientId, out var info))
        {
            info.IsReady = isReady;
            OnPlayerUpdated?.Invoke(info);
        }
    }
    
    public void UpdatePlayerCharacter(ulong clientId, int characterId)
    {
        if (playerInfoMap.TryGetValue(clientId, out var info))
        {
            info.SelectedCharacterId = characterId;
            OnPlayerUpdated?.Invoke(info);
        }
    }

    public PlayerInfo GetPlayerInfo(ulong clientId)
    {
        playerInfoMap.TryGetValue(clientId, out var info);
        return info;
    }

    public IEnumerable<PlayerInfo> GetAllPlayers() => playerInfoMap.Values;

    public bool AreAllPlayersReady()
    {
        if (!playerInfoMap.Any()) return false;
        return playerInfoMap.Values.All(p => p.IsReady && p.SelectedCharacterId != -1);
    }

    public void ClearAllData()
    {
        var clientIds = playerInfoMap.Keys.ToList();
        foreach (var id in clientIds)
        {
            RemovePlayer(id);
        }
        playerInfoMap.Clear();
    }
}