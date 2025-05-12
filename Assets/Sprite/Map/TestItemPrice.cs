using System.Collections.Generic;
using UnityEngine;

public class TestItemPrice : MonoBehaviour
{
    public ItemObject[] item;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartSerch();
        }
    }

    void StartSerch()
    {
        int sum = 0;

        // ← 여기에 추가  
        Dictionary<ItemType, int> typeCounts = new Dictionary<ItemType, int>();

        item = GetComponentsInChildren<ItemObject>();

        foreach (ItemObject io in item)
        {
            ItemData data = io.item.data;
            int price = io.item.price;

            // ← 각 타입별 개수 집계  
            if (typeCounts.ContainsKey(data.itemType))
                typeCounts[data.itemType]++;
            else
                typeCounts[data.itemType] = 1;

            Vector2Int range;
            switch (data.itemType)
            {
                case ItemType.UseItem:
                    range = data.useItemPriceRange;
                    break;
                case ItemType.Normal:
                    range = data.normalPriceRange;
                    break;
                case ItemType.Special:
                    range = data.specialPriceRange;
                    break;
                default:
                    continue;
            }

            if (price < range.x || price > range.y)
                Debug.LogWarning($"[{data.itemName}] 가격({price})이 유효 범위({range.x}~{range.y})를 벗어났습니다.", io);

            sum += price;
        }

        Debug.Log($"전체 아이템 가격 합계: {sum}");

        // ← 집계 결과 출력  
        foreach (var kv in typeCounts)
            Debug.Log($"{kv.Key} 타입: {kv.Value}개");
    }

}
