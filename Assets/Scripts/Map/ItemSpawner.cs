// ItemSpawner.cs (수정된 버전)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class ItemSpawner : NetworkBehaviour
{
    [Header("=== 아이템 부모 설정 ===")]
    [SerializeField] private Transform dropParent;
    
    [Header("=== 아이템 드롭 설정 ===")]
    private const int minTotalPriceSum = 1800;
    private const int maxTotalPriceSum = 2200;

    private void Awake()
    {
        GameManager.instance.itemSpawner = this;
    }

    // SpawnItemsOnMap 메서드의 로직은 거의 동일하지만, Instantiate 부분을 변경합니다.
    public void SpawnItemsOnMap(
        List<List<Vector2Int>> roomItemSpawnPositions,
        List<RoomItemSettings> roomSettings,
        SpreadTilemap spreadTilemap)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[ItemSpawner] 아이템 스폰은 서버에서만 가능합니다.");
            return;
        }
        
        List<ItemData> spawnableItems = ItemDatabase.Instance.GetSpawnableItems();
        if (spawnableItems == null || spawnableItems.Count == 0)
        {
            Debug.LogWarning("[ItemSpawner] 스폰 가능한 아이템이 데이터베이스에 없습니다.");
            return;
        }

        var dropInfos = new List<(ItemData data, Vector3 pos, Vector2 vel, int price)>();
        int totalPriceSum = 0;
        bool reachedMax = false;

        for (int i = 0; i < roomItemSpawnPositions.Count && !reachedMax; i++)
        {
            var spawnPositions = roomItemSpawnPositions[i];
            if (spawnPositions.Count == 0) continue;

            var settings = roomSettings[i];
            int maxDrops = settings != null ? settings.maxDropCount : 1;
            int dropCount = Random.Range(1, maxDrops + 1);

            for (int j = 0; j < dropCount; j++)
            {
                int idx = Random.Range(0, spawnPositions.Count);
                Vector3Int cellPos = (Vector3Int)spawnPositions[idx];
                Vector3 worldPos = spreadTilemap.ItemSpawnTilemap
                                   .CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0f);
                Vector2 velocity = new Vector2(Random.Range(-5f, 5f), Random.Range(15f, 20f));

                float p = Random.value;
                ItemType selectedType = p < 0.25f ? ItemType.UseItem
                                      : p < 0.85f ? ItemType.Normal
                                      : ItemType.Special;

                var candidates = spawnableItems.Where(d => d.itemType == selectedType).ToList();
                if (candidates.Count == 0) continue;
                ItemData data = candidates[Random.Range(0, candidates.Count)];
                int price = Random.Range(data.priceRange.x, data.priceRange.y + 1);

                if (totalPriceSum + price > maxTotalPriceSum)
                {
                    reachedMax = true;
                    break;
                }
                dropInfos.Add((data, worldPos, velocity, price));
                totalPriceSum += price;
            }
        }

        int targetSum = Mathf.Clamp(totalPriceSum, minTotalPriceSum, maxTotalPriceSum);
        float adjustRatio = (totalPriceSum > 0) ? (float)targetSum / totalPriceSum : 0;

        foreach (var (data, pos, vel, price) in dropInfos)
        {
            int adjustedPrice = Mathf.RoundToInt(price * adjustRatio);
            var invItem = new Inventory_Item(data.itemId, adjustedPrice);

            // === 오브젝트 풀링 사용 부분 ===
            var networkObject = NetworkItemPool.Instance.GetObject(pos, Quaternion.identity);
            if(networkObject == null) continue;
            
            var itemObject = networkObject.GetComponent<ItemObject>();
            networkObject.TrySetParent(dropParent, false);
            
            itemObject.SetupAndLaunch(invItem, vel); // 새로운 초기화 메서드 호출
        }

        Debug.Log($"[ItemSpawner] OriginalSum: {totalPriceSum}, ClampedSum: {targetSum}, Items Dropped: {dropInfos.Count}");
    }
    
    public void SpawnSingleItem(Inventory_Item itemToSpawn, Vector3 position)
    {
        if (!IsServer)
        {
            Debug.LogError("[ItemSpawner] SpawnSingleItem은 서버에서만 호출할 수 있습니다.");
            return;
        }

        // === 오브젝트 풀링 사용 부분 ===
        var networkObject = NetworkItemPool.Instance.GetObject(position, Quaternion.identity);
        if(networkObject == null) return;
        
        var itemObject = networkObject.GetComponent<ItemObject>();
        networkObject.TrySetParent(dropParent, false);
        
        Vector2 velocity = new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 6f));
        itemObject.SetupAndLaunch(itemToSpawn, velocity);
        
        Debug.Log($"[ItemSpawner] 단일 아이템 스폰: {itemToSpawn.itemId} at {position}");
    }
    
    public void SpawnPlayerDroppedItem(Inventory_Item itemToSpawn, Vector3 position, Vector2 velocity)
    {
        if (!IsServer)
        {
            Debug.LogError("[ItemSpawner] SpawnPlayerDroppedItem은 서버에서만 호출할 수 있습니다.");
            return;
        }

        // === 오브젝트 풀링 사용 부분 ===
        var networkObject = NetworkItemPool.Instance.GetObject(position, Quaternion.identity);
        if(networkObject == null) return;
        
        var itemObject = networkObject.GetComponent<ItemObject>();
        networkObject.TrySetParent(dropParent, false);

        itemObject.SetupAndLaunch(itemToSpawn, velocity);
        
        Debug.Log($"[ItemSpawner] 플레이어 아이템 드롭: {itemToSpawn.itemId} at {position}");
    }
}