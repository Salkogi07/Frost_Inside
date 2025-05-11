using System.Collections.Generic;
using UnityEngine;

public class ItemDropManager : MonoBehaviour
{
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private List<ItemData> itemList; // Inspector에서 데이터만 할당

    private void Start()
    {
        DropMultipleItems();
    }

    /// <summary>
    /// itemList에 있는 모든 ItemData를 래핑한 InventoryItem으로 만들어서 드롭합니다.
    /// </summary>
    public void DropMultipleItems()
    {
        float offsetX = 2f; // 아이템 간격

        for (int i = 0; i < itemList.Count; i++)
        {
            // 1) 원본 데이터 꺼내기
            ItemData data = itemList[i];

            // 2) 등급별 가격을 랜덤 결정
            int price = GetRandomPrice(data);

            // 3) InventoryItem 생성
            InventoryItem invItem = new InventoryItem(data, price);

            // 4) 위치 계산 및 프리팹 생성
            Vector3 spawnPos = transform.position + new Vector3(i * offsetX, 0f, 0f);
            GameObject newDrop = Instantiate(dropPrefab, spawnPos, Quaternion.identity);

            // 5) 랜덤 초기 속도
            Vector2 randomVel = new Vector2(
                Random.Range(-5f, 5f),
                Random.Range(15f, 20f)
            );

            // 6) InventoryItem을 넘겨서 세팅
            newDrop.GetComponent<ItemObject>().SetupItem(invItem, randomVel);
        }
    }

    /// <summary>
    /// ItemData.itemType에 따라 가격 범위에서 랜덤값을 반환합니다.
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
                // 장비나 사용 아이템 등 기타 타입은 기본 0원
                return 0;
        }
    }
}
