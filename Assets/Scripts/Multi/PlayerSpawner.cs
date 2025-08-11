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
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
        }
    }
    
    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
        
        ulong clientId = sceneEvent.ClientId;
        
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
        {
            return;
        }
        
        Debug.Log($"[Server-Side] Client {clientId} has finished loading the scene. Spawning player.");
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