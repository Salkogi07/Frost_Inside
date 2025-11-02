using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

// 인스펙터에서 풀 설정을 관리하기 위한 클래스
[System.Serializable]
public class PoolConfig
{
    public GameObject prefab;
    public int initialSize;
}

/// <summary>
/// 폭탄, 소환물 등 '사용 아이템' 효과를 위한 네트워크 오브젝트 풀링 시스템입니다.
/// 여러 종류의 프리팹을 등록하여 관리할 수 있습니다.
/// </summary>
public class NetworkUseItemPool : NetworkBehaviour
{
    public static NetworkUseItemPool Instance { get; private set; }

    [Tooltip("풀링된 오브젝트들이 보관될 부모 Transform")]
    [SerializeField] private Transform poolParent;

    [Header("Pool Settings")]
    [Tooltip("풀링할 아이템 효과 프리팹 목록")]
    [SerializeField] private List<PoolConfig> poolConfigs;

    // 각 프리팹별로 오브젝트 큐를 관리하는 딕셔너리
    private Dictionary<GameObject, Queue<NetworkObject>> poolQueues = new Dictionary<GameObject, Queue<NetworkObject>>();
    
    // 생성된 인스턴스가 어떤 원본 프리팹에서 왔는지 추적하기 위한 딕셔너리
    private Dictionary<NetworkObject, GameObject> spawnedObjectsToPrefab = new Dictionary<NetworkObject, GameObject>();


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
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var config in poolConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogWarning("Pool config에 프리팹이 할당되지 않았습니다.");
                continue;
            }

            // 이 프리팹에 대한 큐를 생성합니다.
            var queue = new Queue<NetworkObject>();
            poolQueues.Add(config.prefab, queue);

            for (int i = 0; i < config.initialSize; i++)
            {
                var instance = Instantiate(config.prefab, poolParent);
                var networkObject = instance.GetComponent<NetworkObject>();
                
                // 스폰은 하되, 클라이언트에게 활성화 상태로 보이게 하지는 않습니다.
                networkObject.Spawn(false); 
                
                // 생성 직후 바로 풀에 반환하여 비활성화 상태로 만듭니다.
                ReturnObjectToPool(networkObject, config.prefab);
            }
        }
    }

    /// <summary>
    /// (서버 전용) 지정된 프리팹에 해당하는 오브젝트를 풀에서 가져옵니다.
    /// </summary>
    public NetworkObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return null;

        if (!poolQueues.ContainsKey(prefab))
        {
            Debug.LogError($"{prefab.name} 프리팹은 풀에 등록되지 않았습니다!");
            return null;
        }

        NetworkObject networkObject;
        var queue = poolQueues[prefab];

        if (queue.Count > 0)
        {
            networkObject = queue.Dequeue();
        }
        else
        {
            // 풀이 비었다면 새로 생성합니다.
            var instance = Instantiate(prefab, poolParent);
            networkObject = instance.GetComponent<NetworkObject>();
            networkObject.Spawn(false);
        }
        
        // 위치와 회전을 설정하고 활성화합니다.
        networkObject.transform.SetPositionAndRotation(position, rotation);
        // 부모를 임시로 null로 설정하여 월드 좌표계에서 자유롭게 움직이게 합니다.
        networkObject.transform.SetParent(null, worldPositionStays: true);
        networkObject.gameObject.GetComponent<ActiveStateSynchronizer>().SetActiveState(true);

        // 어떤 프리팹에서 생성되었는지 기록합니다.
        spawnedObjectsToPrefab.Add(networkObject, prefab);

        return networkObject;
    }

    /// <summary>
    /// (서버 전용) 사용이 끝난 오브젝트를 풀에 반환합니다.
    /// </summary>
    public void ReturnObjectToPool(NetworkObject networkObject)
    {
        if (!IsServer) return;

        // 이 오브젝트가 어떤 원본 프리팹에서 생성되었는지 찾습니다.
        if (spawnedObjectsToPrefab.TryGetValue(networkObject, out GameObject prefab))
        {
            ReturnObjectToPool(networkObject, prefab);
            spawnedObjectsToPrefab.Remove(networkObject);
        }
        else
        {
            Debug.LogWarning($"{networkObject.name}는 이 풀에서 관리하는 오브젝트가 아닙니다. Despawn 후 Destroy합니다.");
            networkObject.Despawn();
        }
    }
    
    // 내부적으로 사용될 반환 로직
    private void ReturnObjectToPool(NetworkObject networkObject, GameObject prefab)
    {
        networkObject.gameObject.GetComponent<ActiveStateSynchronizer>().SetActiveState(false);
        networkObject.transform.SetParent(poolParent);
        
        if(poolQueues.TryGetValue(prefab, out var queue))
        {
            queue.Enqueue(networkObject);
        }
    }
}