using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class NetworkMonsterPool : NetworkBehaviour
{
    public static NetworkMonsterPool Instance { get; private set; }

    [Tooltip("풀링된 몬스터들이 보관될 부모 Transform")]
    [SerializeField] private Transform poolParent;

    [Header("Pool Settings")]
    [Tooltip("풀링할 모든 몬스터 프리팹 리스트")]
    [SerializeField] private List<GameObject> monsterPrefabs;

    [Tooltip("종류별로 초기에 생성해 둘 몬스터 수")]
    [SerializeField] private int initialPoolSize = 10;

    // Prefab의 고유 ID를 Key로 사용하여 몬스터 풀을 관리
    private Dictionary<ulong, Queue<NetworkObject>> objectPool = new Dictionary<ulong, Queue<NetworkObject>>();

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

    private void InitializePool()
    {
        foreach (var prefab in monsterPrefabs)
        {
            if (prefab == null) continue;
            
            var networkPrefab = prefab.GetComponent<NetworkObject>();
            if (networkPrefab == null)
            {
                Debug.LogError($"[NetworkMonsterPool] Prefab '{prefab.name}' does not have a NetworkObject component.");
                continue;
            }

            ulong prefabId = networkPrefab.NetworkObjectId; 
            objectPool[prefabId] = new Queue<NetworkObject>();

            for (int i = 0; i < initialPoolSize; i++)
            {
                var instance = Instantiate(prefab, poolParent);
                var networkObject = instance.GetComponent<NetworkObject>();
                networkObject.Spawn(false);
                ReturnObjectToPool(networkObject);
            }
        }
    }

    public NetworkObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return null;

        var networkPrefab = prefab.GetComponent<NetworkObject>();
        if (networkPrefab == null) return null;
        
        ulong prefabId = networkPrefab.NetworkObjectId;

        if (objectPool.TryGetValue(prefabId, out var pool) && pool.Count > 0)
        {
            var networkObject = pool.Dequeue();
            networkObject.transform.SetPositionAndRotation(position, rotation);
            networkObject.gameObject.SetActive(true);
            return networkObject;
        }
        else
        {
            // 해당 프리팹의 풀이 비었거나 존재하지 않을 경우 새로 생성
            var instance = Instantiate(prefab, position, rotation, poolParent);
            var networkObject = instance.GetComponent<NetworkObject>();
            networkObject.Spawn(true);
            return networkObject;
        }
    }

    public void ReturnObjectToPool(NetworkObject networkObject)
    {
        if (!IsServer || networkObject == null) return;
        
        ulong prefabId = networkObject.NetworkObjectId;

        if (!objectPool.ContainsKey(prefabId))
        {
            objectPool[prefabId] = new Queue<NetworkObject>();
        }

        networkObject.transform.SetParent(poolParent, worldPositionStays: false);
        networkObject.gameObject.SetActive(false);
        objectPool[prefabId].Enqueue(networkObject);
    }
}