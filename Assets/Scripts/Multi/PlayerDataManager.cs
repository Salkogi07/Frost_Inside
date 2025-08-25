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
    
    // --- 로딩 상태 추적을 위한 변수 추가 ---
    private readonly HashSet<ulong> clientsLoadedScene = new HashSet<ulong>();

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
    
    //디버깅용
    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (var player in playerInfoMap)
            {
                Debug.Log(player.Value.SteamName);
                Debug.Log(player.Value.SelectedCharacterId);
            }
        }*/

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (var player in clientsLoadedScene)
            {
                Debug.Log(player);
            }
        }
    }
    
    /// <summary>
    /// (서버 전용) 씬을 로드한 클라이언트를 등록합니다.
    /// </summary>
    public void AddLoadedClient(ulong clientId)
    {
        Debug.Log($"[PlayerDataManager] Client {clientId} loaded scene.");
        clientsLoadedScene.Add(clientId);
    }

    /// <summary>
    /// (서버 전용) 로드된 클라이언트 목록을 초기화합니다.
    /// </summary>
    public void ClearLoadedClients()
    {
        clientsLoadedScene.Clear();
    }

    /// <summary>
    /// (서버 전용) 로드된 클라이언트의 수를 반환합니다.
    /// </summary>
    public int GetLoadedClientCount()
    {
        return clientsLoadedScene.Count;
    }

    public void AddPlayer(ulong clientId, ulong steamId, string steamName)
    {
        if (playerInfoMap.ContainsKey(clientId)) return;

        var newPlayer = new PlayerInfo
        {
            ClientId = clientId,
            SteamId = steamId,
            SteamName = steamName,
            IsReady = true,
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