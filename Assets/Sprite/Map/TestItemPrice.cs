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

        // �� ���⿡ �߰�  
        Dictionary<ItemType, int> typeCounts = new Dictionary<ItemType, int>();

        item = GetComponentsInChildren<ItemObject>();

        foreach (ItemObject io in item)
        {
            ItemData data = io.item.data;
            int price = io.item.price;

            // �� �� Ÿ�Ժ� ���� ����  
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
                Debug.LogWarning($"[{data.itemName}] ����({price})�� ��ȿ ����({range.x}~{range.y})�� ������ϴ�.", io);

            sum += price;
        }

        Debug.Log($"��ü ������ ���� �հ�: {sum}");

        // �� ���� ��� ���  
        foreach (var kv in typeCounts)
            Debug.Log($"{kv.Key} Ÿ��: {kv.Value}��");
    }

}
