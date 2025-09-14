using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class NetworkEnemyPool : NetworkBehaviour
{
    public static NetworkEnemyPool Instance { get; private set; }

    [Tooltip("풀링된 몬스터들이 보관될 부모 Transform")]
    [SerializeField] private Transform poolParent;

    [Header("Pool Settings")]
    [Tooltip("풀링할 모든 몬스터 프리팹 리스트")]
    [SerializeField] private List<GameObject> enemyPrefabs;

    [Tooltip("종류별로 초기에 생성해 둘 몬스터 수")]
    [SerializeField] private int initialPoolSize = 10;

    // Key를 GameObject (Prefab) 자체로 사용하여 몬스터 풀을 관리
    private Dictionary<GameObject, Queue<NetworkObject>> objectPool = new Dictionary<GameObject, Queue<NetworkObject>>();
    // 스폰된 NetworkObject의 ID를 통해 원본 프리팹을 찾기 위한 맵
    private Dictionary<ulong, GameObject> _networkIdToPrefabMap = new Dictionary<ulong, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (poolParent == null)
        {
            poolParent = transform;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        InitializePool();
    }
    
    /// <summary>
    /// 이 풀이 관리하는 모든 몬스터 프리팹의 읽기 전용 리스트를 반환합니다.
    /// MonsterSpawner가 스폰할 몬스터 종류를 조회하기 위해 사용합니다.
    /// </summary>
    public IReadOnlyList<GameObject> GetAvailablePrefabs()
    {
        return enemyPrefabs;
    }
    
    private void InitializePool()
    {
        foreach (var prefab in enemyPrefabs)
        {
            if (prefab == null) continue;
            
            if (prefab.GetComponent<NetworkObject>() == null)
            {
                Debug.LogError($"[NetworkMonsterPool] Prefab '{prefab.name}' does not have a NetworkObject component.");
                continue;
            }

            // 프리팹을 Key로 사용하여 큐 생성
            objectPool[prefab] = new Queue<NetworkObject>();

            for (int i = 0; i < initialPoolSize; i++)
            {
                var instance = Instantiate(prefab, poolParent);
                var networkObject = instance.GetComponent<NetworkObject>();
                networkObject.Spawn(false);
                
                // 스폰된 인스턴스의 ID와 원본 프리팹을 매핑
                _networkIdToPrefabMap[networkObject.NetworkObjectId] = prefab;
                
                ReturnObjectToPool(networkObject);
            }
        }
    }

    public NetworkObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return null;

        if (prefab.GetComponent<NetworkObject>() == null) return null;
        
        // 프리팹을 Key로 사용하여 해당하는 풀을 찾음
        if (objectPool.TryGetValue(prefab, out var pool) && pool.Count > 0)
        {
            var networkObject = pool.Dequeue();
            networkObject.transform.SetPositionAndRotation(position, rotation);
            networkObject.gameObject.GetComponent<ActiveStateSynchronizer>().SetActiveState(true);
            return networkObject;
        }
        else
        {
            // 해당 프리팹의 풀이 비었거나 존재하지 않을 경우 새로 생성
            var instance = Instantiate(prefab, position, rotation, poolParent);
            var networkObject = instance.GetComponent<NetworkObject>();
            networkObject.Spawn(false);
            
            // 동적으로 생성된 인스턴스도 ID와 원본 프리팹을 매핑
            _networkIdToPrefabMap[networkObject.NetworkObjectId] = prefab;
            networkObject.transform.SetParent(poolParent, worldPositionStays: false);
            
            return networkObject;
        }
    }

    public void ReturnObjectToPool(NetworkObject networkObject)
    {
        if (!IsServer || networkObject == null) return;
        
        // NetworkObjectId를 사용해 원본 프리팹을 찾음
        if (_networkIdToPrefabMap.TryGetValue(networkObject.NetworkObjectId, out GameObject prefab))
        {
            networkObject.transform.SetParent(poolParent, worldPositionStays: false);
            networkObject.gameObject.GetComponent<ActiveStateSynchronizer>().SetActiveState(false);
            
            // 찾은 프리팹에 해당하는 풀에 인스턴스를 반환
            if (objectPool.TryGetValue(prefab, out var pool))
            {
                pool.Enqueue(networkObject);
            }
            else
            {
                 Debug.LogWarning($"[NetworkEnemyPool] Pool for prefab '{prefab.name}' not found. Creating a new one.");
                 var newPool = new Queue<NetworkObject>();
                 newPool.Enqueue(networkObject);
                 objectPool[prefab] = newPool;
            }
        }
        else
        {
            Debug.LogError($"[NetworkEnemyPool] Could not find original prefab for NetworkObject ID {networkObject.NetworkObjectId}. The object will be destroyed instead of pooled.");
            Destroy(networkObject.gameObject);
        }
    }
}