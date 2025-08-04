// --- START OF MODIFIED FILE PlayerSpawner.cs (Using Arrays) ---

using Unity.Netcode;
using UnityEngine;
using System.Linq; // Keep this for .TryGetValue on Dictionary

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private int nextSpawnPointIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayerForClient(client.ClientId);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject == null)
        {
             SpawnPlayerForClient(clientId);
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected.");
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        if (!IsServer) return;

        PlayerInfo playerInfo = PlayerDataManager.instance.GetPlayerInfo(clientId);
        if (playerInfo == null)
        {
            Debug.LogError($"[PlayerSpawner] Cannot find PlayerInfo for client {clientId}. Player will not be spawned.");
            return;
        }

        GameObject prefabToSpawn = GetPrefabForCharacter(playerInfo.SelectedCharacterId);
        Vector3 spawnPosition = GetNextSpawnPosition();

        GameObject playerInstance = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        
        networkObject.SpawnAsPlayerObject(clientId, true);
        
        Debug.Log($"Spawned character {prefabToSpawn.name} for Client ID: {clientId} at {spawnPosition}");
    }
    
    private GameObject GetPrefabForCharacter(int characterId)
    {
        // 배열의 길이를 기준으로 유효성 검사 (list.Count -> array.Length)
        if (characterId < 0 || characterId >= playerPrefabs.Length)
        {
            Debug.LogWarning($"Invalid character ID: {characterId}. Spawning default character (index 0).");
            return playerPrefabs[0];
        }
        
        return playerPrefabs[characterId];
    }
    
    private Vector3 GetNextSpawnPosition()
    {
        // 배열의 길이를 기준으로 유효성 검사
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned. Spawning at world origin (0,0,0).");
            return Vector3.zero;
        }

        Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
        
        // 배열의 길이를 사용한 모듈러 연산
        nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;

        return spawnPoint.position;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton != null)
            {
                 NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                 NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }
    }
}