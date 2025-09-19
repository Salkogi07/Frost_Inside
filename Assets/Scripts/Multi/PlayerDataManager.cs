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
    private readonly HashSet<ulong> clientsMapGenerated = new HashSet<ulong>();
    private readonly HashSet<ulong> clientsPlayerSpawned = new HashSet<ulong>();

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

        /*if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (var player in clientsLoadedScene)
            {
                Debug.Log(player);
            }
        }*/
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
    /// (서버 전용) 맵 생성을 완료한 클라이언트를 등록합니다.
    /// </summary>
    public void AddMapGeneratedClient(ulong clientId)
    {
        Debug.Log($"[PlayerDataManager] Client {clientId} finished map generation.");
        clientsMapGenerated.Add(clientId);
    }
    
    /// <summary>
    /// (서버 전용) 플레이어 스폰을 완료한 클라이언트를 등록합니다.
    /// </summary>
    public void AddPlayerSpawnedClient(ulong clientId)
    {
        Debug.Log($"[PlayerDataManager] Client {clientId} has its player spawned.");
        clientsPlayerSpawned.Add(clientId);
    }

    /// <summary>
    /// (서버 전용) 로드된 클라이언트 목록을 초기화합니다.
    /// </summary>
    public void ClearLoadedClients()
    {
        clientsLoadedScene.Clear();
    }
    
    /// <summary>
    /// (서버 전용) 맵 생성을 완료한 클라이언트 목록을 초기화합니다.
    /// </summary>
    public void ClearMapGeneratedClients()
    {
        clientsMapGenerated.Clear();
    }
    
    /// <summary>
    /// (서버 전용) 플레이어 스폰을 완료한 클라이언트 목록을 초기화합니다.
    /// </summary>
    public void ClearPlayerSpawnedClients()
    {
        clientsPlayerSpawned.Clear();
    }

    /// <summary>
    /// (서버 전용) 로드된 클라이언트의 수를 반환합니다.
    /// </summary>
    public int GetLoadedClientCount()
    {
        return clientsLoadedScene.Count;
    }
    
    /// <summary>
    /// (서버 전용) 맵 생성을 완료한 클라이언트의 수를 반환합니다.
    /// </summary>
    public int GetMapGeneratedClientCount()
    {
        return clientsMapGenerated.Count;
    }
    
    /// <summary>
    /// (서버 전용) 플레이어 스폰을 완료한 클라이언트의 수를 반환합니다.
    /// </summary>
    public int GetPlayerSpawnedClientCount()
    {
        return clientsPlayerSpawned.Count;
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
            SelectedCharacterId = 0,
            IsDead = false
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
    
    public void UpdatePlayerDeadStatus(ulong clientId, bool isDead)
    {
        if (playerInfoMap.TryGetValue(clientId, out var info))
        {
            info.IsDead = isDead;
            OnPlayerUpdated?.Invoke(info);
            Debug.Log($"[PlayerDataManager] Player {info.SteamName} dead status updated to: {isDead}");
        }
    }

    public PlayerInfo GetPlayerInfo(ulong clientId)
    {
        playerInfoMap.TryGetValue(clientId, out var info);
        return info;
    }
    
    public GameObject GetPlayerObject(ulong clientId)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                return client.PlayerObject != null ? client.PlayerObject.gameObject : null;
            }
        }
        return null;
    }

    public List<GameObject> GetAllPlayerObjects()
    {
        var playerObjects = new List<GameObject>();
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("[PlayerDataManager] GetAllPlayerObjects can only be called on the server.");
            return playerObjects;
        }

        foreach (var clientId in playerInfoMap.Keys)
        {
            GameObject playerObj = GetPlayerObject(clientId);
            if (playerObj != null)
            {
                playerObjects.Add(playerObj);
            }
        }
        return playerObjects;
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
        clientsLoadedScene.Clear();
        clientsMapGenerated.Clear();
        clientsPlayerSpawned.Clear();
    }
}