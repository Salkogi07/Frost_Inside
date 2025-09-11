using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// 네트워크 오브젝트를 위한 간단한 오브젝트 풀링 시스템입니다.
/// 서버에서만 오브젝트를 생성하고 관리합니다.
/// </summary>
public class NetworkItemPool : NetworkBehaviour
{
    public static NetworkItemPool Instance { get; private set; }
    
    [Tooltip("풀링된 오브젝트들이 보관될 부모 Transform")]
    [SerializeField] private Transform poolParent;

    [Header("Pool Settings")]
    [Tooltip("풀링할 네트워크 오브젝트 프리팹")]
    [SerializeField] private GameObject itemPrefab;

    [Tooltip("초기에 생성해 둘 오브젝트 수")]
    [SerializeField] private int initialPoolSize = 20;

    private Queue<NetworkObject> objectPool = new Queue<NetworkObject>();

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

        // 만약 부모가 지정되지 않았다면, 이 오브젝트의 Transform을 부모로 사용합니다.
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
        for (int i = 0; i < initialPoolSize; i++)
        {
            var instance = Instantiate(itemPrefab, poolParent);
            var networkObject = instance.GetComponent<NetworkObject>();
            networkObject.Spawn(false);
            ReturnObjectToPool(networkObject);
        }
    }

    public NetworkObject GetObject(Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return null;

        NetworkObject networkObject;
        if (objectPool.Count > 0)
        {
            networkObject = objectPool.Dequeue();
            networkObject.transform.SetPositionAndRotation(position, rotation);
            networkObject.gameObject.SetActive(true);
        }
        else
        {
            // 풀이 비었을 때 새로 생성하는 경우에도 부모를 지정합니다.
            var instance = Instantiate(itemPrefab, position, rotation, poolParent);
            networkObject = instance.GetComponent<NetworkObject>();
            networkObject.Spawn(true);
        }
        
        return networkObject;
    }

    public void ReturnObjectToPool(NetworkObject networkObject)
    {
        if (!IsServer) return;
        
        networkObject.transform.SetParent(poolParent, worldPositionStays: false);
        networkObject.gameObject.SetActive(false);
        objectPool.Enqueue(networkObject);
    }
}