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

    [Tooltip("드롭할 프리팹을 할당하세요.")]
    [SerializeField] private GameObject dropPrefab;

    // [수정됨] ItemSpawner가 자체적으로 리스트를 갖는 대신 데이터베이스를 사용하므로 이 줄을 삭제합니다.
    // [SerializeField] private List<ItemData> itemList; 

    /// <summary>
    /// 서버에서만 호출되어 맵에 아이템을 스폰합니다.
    /// MakeRandomMap 스크립트가 맵 생성을 완료한 후 이 메서드를 호출합니다.
    /// </summary>
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

        // ItemDatabase 싱글턴 인스턴스에서 스폰 가능한 아이템 리스트를 불러옵니다.
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

                // 데이터베이스에서 받아온 spawnableItems 리스트에서 아이템을 필터링합니다.
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
            
            // 데이터베이스에서 불러온 아이템 데이터의 고유 ID(itemId)를 사용하여 Inventory_Item을 생성합니다.
            var invItem = new Inventory_Item(data.itemId, adjustedPrice);

            var drop = Instantiate(dropPrefab, pos, Quaternion.identity, dropParent);
            var itemObject = drop.GetComponent<ItemObject>();
            var networkObject = drop.GetComponent<NetworkObject>();
            
            networkObject.Spawn(true);
            networkObject.TrySetParent(dropParent, false);
            
            itemObject.SetupItemServerRpc(invItem, vel);
        }

        Debug.Log($"[ItemSpawner] OriginalSum: {totalPriceSum}, ClampedSum: {targetSum}, Items Dropped: {dropInfos.Count}");
    }
}