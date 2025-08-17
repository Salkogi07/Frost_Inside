// PlayerSpawner.cs (수정된 버전)

using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private int nextSpawnPointIndex = 0;
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        PlayerDataManager.instance.OnPlayerAdded += HandlePlayerAdded;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
        }
        
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.OnPlayerAdded -= HandlePlayerAdded;
        }
    }
    
    private void HandlePlayerAdded(PlayerInfo playerInfo)
    {
        if (!IsServer) return;
        
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerInfo.ClientId, out var client) && client.PlayerObject != null)
        {
            Debug.LogWarning($"[PlayerSpawner] Player object for client {playerInfo.ClientId} already exists. Skipping spawn in HandlePlayerAdded.");
            return;
        }
        
        Debug.Log($"[Server-Side] OnPlayerAdded event received for client {playerInfo.ClientId}. Spawning player.");
        SpawnPlayerForClient(playerInfo.ClientId);
    }
    
    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
        
        ulong clientId = sceneEvent.ClientId;

        if (PlayerDataManager.instance.GetPlayerInfo(clientId) == null)
        {
            return;
        }
        
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
        {
            return;
        }
        
        Debug.Log($"[Server-Side] Client {clientId} has finished loading the scene AND player data is ready. Spawning player.");
        SpawnPlayerForClient(clientId);
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        if (!IsServer) return;
        
        PlayerInfo playerInfo = PlayerDataManager.instance.GetPlayerInfo(clientId);
        if (playerInfo == null)
        {
            Debug.LogError($"[PlayerSpawner] CRITICAL: Cannot find PlayerInfo for client {clientId}. Player will not be spawned.");
            return;
        }

        GameObject prefabToSpawn = GetPrefabForCharacter(playerInfo.SelectedCharacterId);
        if (prefabToSpawn == null)
        {
            Debug.LogError($"[PlayerSpawner] CRITICAL: GetPrefabForCharacter returned null. Spawning aborted.");
            return;
        }
        
        Vector3 spawnPosition = GetNextSpawnPosition();
        GameObject playerInstance = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        
        if (playerInstance.TryGetComponent<PlayerNameSetup>(out var playerNameSetup))
        {
            playerNameSetup.SetPlayerName(playerInfo.SteamName);
        }
    
        networkObject.SpawnAsPlayerObject(clientId, true);
    
        Debug.Log($"Spawned character {prefabToSpawn.name} for Client ID: {clientId} at {spawnPosition}");
    }
    
    /// <summary>
    /// (서버 전용) 지정된 클라이언트의 플레이어 캐릭터를 새로운 캐릭터로 교체합니다.
    /// </summary>
    /// <param name="clientId">캐릭터를 변경할 클라이언트의 ID</param>
    /// <param name="newCharacterId">새로운 캐릭터 프리팹의 인덱스</param>
    public void RespawnPlayerCharacter(ulong clientId, int newCharacterId)
    {
        if (!IsServer) return;

        // 기존 플레이어 오브젝트를 찾습니다.
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
        {
            Vector3 oldPosition = client.PlayerObject.transform.position;
            Quaternion oldRotation = client.PlayerObject.transform.rotation;

            // 기존 오브젝트를 Despawn하고 파괴합니다.
            client.PlayerObject.Despawn(true);

            // 새로운 캐릭터를 이전 위치에 스폰합니다.
            SpawnNewCharacterForClient(clientId, newCharacterId, oldPosition, oldRotation);
        }
        else
        {
            Debug.LogWarning($"[PlayerSpawner] Client {clientId} has no player object to respawn. Attempting initial spawn.");
            // 만약의 경우 플레이어 오브젝트가 없다면, 그냥 새로 스폰합니다.
            SpawnPlayerForClient(clientId);
        }
    }

    private void SpawnNewCharacterForClient(ulong clientId, int characterId, Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return;

        PlayerInfo playerInfo = PlayerDataManager.instance.GetPlayerInfo(clientId);
        if (playerInfo == null)
        {
            Debug.LogError($"[PlayerSpawner] CRITICAL: Cannot find PlayerInfo for client {clientId} during respawn.");
            return;
        }

        GameObject prefabToSpawn = GetPrefabForCharacter(characterId);
        if (prefabToSpawn == null)
        {
            Debug.LogError($"[PlayerSpawner] CRITICAL: GetPrefabForCharacter returned null for new character ID {characterId}.");
            return;
        }

        GameObject playerInstance = Instantiate(prefabToSpawn, position, rotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (playerInstance.TryGetComponent<PlayerNameSetup>(out var playerNameSetup))
        {
            playerNameSetup.SetPlayerName(playerInfo.SteamName);
        }

        networkObject.SpawnAsPlayerObject(clientId, true);

        Debug.Log($"Respawned character {prefabToSpawn.name} for Client ID: {clientId} at {position}");
    }
    
    private GameObject GetPrefabForCharacter(int characterId)
    {
        if (characterId < 0 || characterId >= playerPrefabs.Length)
        {
            Debug.LogWarning($"Invalid character ID: {characterId}. Spawning default character (index 0).");
            if (playerPrefabs.Length > 0)
            {
                return playerPrefabs[0];
            }
            Debug.LogError("Player Prefabs array is empty!");
            return null;
        }
        
        return playerPrefabs[characterId];
    }
    
    private Vector3 GetNextSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned. Spawning at world origin (0,0,0).");
            return Vector3.zero;
        }

        Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
        nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;

        return spawnPoint.position;
    }
}