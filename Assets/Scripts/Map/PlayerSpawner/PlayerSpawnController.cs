using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnController : NetworkBehaviour
{
    public static PlayerSpawnController instance;

    [Header("모든 플레이어 프리팹")]
    [Tooltip("게임에서 사용될 모든 플레이어 캐릭터 프리팹 배열")]
    [SerializeField] private GameObject[] playerPrefabs;

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

    /// <summary>
    /// (서버 전용) 지정된 클라이언트의 플레이어를 현재 씬의 스폰 지점에 스폰합니다.
    /// </summary>
    public void SpawnPlayerForClient(ulong clientId)
    {
        if (!IsServer) return;
        
        // 1. 이미 플레이어가 있는지 확인
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
        {
            Debug.LogWarning($"[Spawn Controller] 클라이언트 {clientId}의 플레이어는 이미 존재하여 스폰을 건너뜁니다.");
            return;
        }

        // 2. 스폰 위치 가져오기
        SpawnPointManager spawnPointManager = SpawnPointManager.instance;
        if (spawnPointManager == null)
        {
            Debug.LogError("[Spawn Controller] 현재 씬에서 SpawnPointManager를 찾을 수 없습니다! 스폰을 중단합니다.");
            return;
        }
        Vector3 spawnPosition = spawnPointManager.GetNextSpawnPoint().position;
        
        // 3. 플레이어 정보 및 프리팹 가져오기
        PlayerInfo playerInfo = PlayerDataManager.instance.GetPlayerInfo(clientId);
        if (playerInfo == null)
        {
            Debug.LogError($"[Spawn Controller] 클라이언트 {clientId}의 PlayerInfo가 없어 스폰할 수 없습니다.");
            return;
        }
        GameObject prefabToSpawn = GetPrefabForCharacter(playerInfo.SelectedCharacterId);
        if (prefabToSpawn == null) return;

        // 4. 스폰 실행
        GameObject playerInstance = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        
        Debug.Log($"[Spawn Controller] {playerInfo.SteamName}을(를) {spawnPosition}에 스폰 완료.");
    }
    
    /// <summary>
    /// (서버 전용) 지정된 클라이언트의 캐릭터를 교체합니다. 기존 위치에 새로 스폰합니다.
    /// </summary>
    public void RespawnPlayerCharacter(ulong clientId, int newCharacterId)
    {
        if (!IsServer) return;

        // 1. 기존 플레이어 오브젝트를 찾습니다.
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
        {
            // 2. 기존 위치와 회전값을 저장합니다.
            Vector3 oldPosition = client.PlayerObject.transform.position;
            Quaternion oldRotation = client.PlayerObject.transform.rotation;

            // 3. 기존 오브젝트를 네트워크에서 Despawn하고 파괴합니다.
            client.PlayerObject.Despawn(true);

            // 4. 새로운 캐릭터를 이전 위치에 스폰하도록 헬퍼 함수를 호출합니다.
            SpawnNewCharacterForClient(clientId, newCharacterId, oldPosition, oldRotation);
        }
        else
        {
            Debug.LogWarning($"[Spawn Controller] 클라이언트 {clientId}의 플레이어 오브젝트가 없어 리스폰할 수 없습니다. 대신 일반 스폰을 시도합니다.");
            // 만약의 경우 플레이어 오브젝트가 없다면, 그냥 새로 스폰합니다.
            SpawnPlayerForClient(clientId);
        }
    }

    /// <summary>
    /// (서버 전용) 지정된 위치에 새로운 캐릭터를 스폰하는 헬퍼 함수입니다.
    /// </summary>
    private void SpawnNewCharacterForClient(ulong clientId, int characterId, Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return;

        PlayerInfo playerInfo = PlayerDataManager.instance.GetPlayerInfo(clientId);
        if (playerInfo == null) return;
        
        GameObject prefabToSpawn = GetPrefabForCharacter(characterId);
        if (prefabToSpawn == null) return;
        
        GameObject playerInstance = Instantiate(prefabToSpawn, position, rotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);

        Debug.Log($"[Spawn Controller] {playerInfo.SteamName}의 캐릭터를 {prefabToSpawn.name}(으)로 교체하여 {position}에 리스폰했습니다.");
    }

    private GameObject GetPrefabForCharacter(int characterId)
    {
        if (characterId < 0 || characterId >= playerPrefabs.Length)
        {
            Debug.LogWarning($"잘못된 캐릭터 ID: {characterId}. 기본 캐릭터(0)를 스폰합니다.");
            if (playerPrefabs.Length > 0) return playerPrefabs[0];
            
            Debug.LogError("PlayerPrefabs 배열이 비어있습니다!");
            return null;
        }
        return playerPrefabs[characterId];
    }
}