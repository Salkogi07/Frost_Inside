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

        // [수정됨] PlayerDataManager에 해당 클라이언트의 정보가 등록되었는지 먼저 확인합니다.
        // 정보가 없다면, 아직 AnnounceMyselfToServerRpc가 도착하지 않은 것이므로 스폰을 시도하지 않고 기다립니다.
        // 잠시 후 HandlePlayerAdded가 스폰을 처리해 줄 것입니다.
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
            // 이 수정으로 인해 이 에러는 거의 발생하지 않아야 합니다.
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