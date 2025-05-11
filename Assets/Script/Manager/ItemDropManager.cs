using System.Collections.Generic;
using UnityEngine;

public class ItemDropManager : MonoBehaviour
{
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private List<ItemData> itemList; // Inspector���� �����͸� �Ҵ�

    private void Start()
    {
        DropMultipleItems();
    }

    /// <summary>
    /// itemList�� �ִ� ��� ItemData�� ������ InventoryItem���� ���� ����մϴ�.
    /// </summary>
    public void DropMultipleItems()
    {
        float offsetX = 2f; // ������ ����

        for (int i = 0; i < itemList.Count; i++)
        {
            // 1) ���� ������ ������
            ItemData data = itemList[i];

            // 2) ��޺� ������ ���� ����
            int price = GetRandomPrice(data);

            // 3) InventoryItem ����
            InventoryItem invItem = new InventoryItem(data, price);

            // 4) ��ġ ��� �� ������ ����
            Vector3 spawnPos = transform.position + new Vector3(i * offsetX, 0f, 0f);
            GameObject newDrop = Instantiate(dropPrefab, spawnPos, Quaternion.identity);

            // 5) ���� �ʱ� �ӵ�
            Vector2 randomVel = new Vector2(
                Random.Range(-5f, 5f),
                Random.Range(15f, 20f)
            );

            // 6) InventoryItem�� �Ѱܼ� ����
            newDrop.GetComponent<ItemObject>().SetupItem(invItem, randomVel);
        }
    }

    /// <summary>
    /// ItemData.itemType�� ���� ���� �������� �������� ��ȯ�մϴ�.
    /// </summary>
    private int GetRandomPrice(ItemData data)
    {
        switch (data.itemType)
        {
            case ItemType.Normal:
                return Random.Range(
                    data.normalPriceRange.x,
                    data.normalPriceRange.y + 1
                );
            case ItemType.Special:
                return Random.Range(
                    data.specialPriceRange.x,
                    data.specialPriceRange.y + 1
                );
            case ItemType.Natural:
                return Random.Range(
                    data.naturalPriceRange.x,
                    data.naturalPriceRange.y + 1
                );
            default:
                // ��� ��� ������ �� ��Ÿ Ÿ���� �⺻ 0��
                return 0;
        }
    }
}
