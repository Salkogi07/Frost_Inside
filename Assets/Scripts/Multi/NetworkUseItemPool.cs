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

    [Tooltip("풀링된 오브젝트들이 보관될 부모 Transform. 이 오브젝트에는 반드시 NetworkObject 컴포넌트가 있어야 합니다.")]
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
        // 서버에서만 풀을 초기화합니다.
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

            var queue = new Queue<NetworkObject>();
            poolQueues.Add(config.prefab, queue);

            for (int i = 0; i < config.initialSize; i++)
            {
                var instance = Instantiate(config.prefab, poolParent);
                var networkObject = instance.GetComponent<NetworkObject>();
                
                networkObject.Spawn(false); 
                
                ReturnObjectToPoolInternal(networkObject, config.prefab);
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
            var instance = Instantiate(prefab);
            networkObject = instance.GetComponent<NetworkObject>();
            networkObject.Spawn(false);
        }
        
        // [수정된 부분 1] null을 Transform으로 명시하고, worldPositionStays를 true로 설정
        networkObject.TrySetParent(null as Transform, true);
        networkObject.transform.SetPositionAndRotation(position, rotation);
        
        networkObject.gameObject.GetComponent<ActiveStateSynchronizer>().SetActiveState(true);

        spawnedObjectsToPrefab.Add(networkObject, prefab);

        return networkObject;
    }

    /// <summary>
    /// (서버 전용) 사용이 끝난 오브젝트를 풀에 반환합니다.
    /// </summary>
    public void ReturnObjectToPool(NetworkObject networkObject)
    {
        if (!IsServer) return;

        if (spawnedObjectsToPrefab.TryGetValue(networkObject, out GameObject prefab))
        {
            ReturnObjectToPoolInternal(networkObject, prefab);
            spawnedObjectsToPrefab.Remove(networkObject);
        }
        else
        {
            Debug.LogWarning($"{networkObject.name}는 이 풀에서 관리하는 오브젝트가 아닙니다. Despawn 후 Destroy합니다.");
            networkObject.Despawn(true);
        }
    }
    
    // 내부적으로 사용될 반환 로직
    private void ReturnObjectToPoolInternal(NetworkObject networkObject, GameObject prefab)
    {
        networkObject.gameObject.GetComponent<ActiveStateSynchronizer>().SetActiveState(false);
        
        // [수정된 부분 2] worldPositionStays를 false로 설정하여 부모 기준 로컬 좌표를 리셋
        networkObject.TrySetParent(poolParent, false);
        
        if(poolQueues.TryGetValue(prefab, out var queue))
        {
            queue.Enqueue(networkObject);
        }
        else
        {
             Debug.LogWarning($"{prefab.name}에 해당하는 큐를 찾을 수 없어 {networkObject.name}를 파괴합니다.");
             networkObject.Despawn(true);
        }
    }
}