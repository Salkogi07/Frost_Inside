using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData data;
    public int price;  // ▲ 개별 아이템의 가격을 저장할 필드 추가

    // 기존 생성자: 아이템 데이터만 받아 초기화
    public InventoryItem(ItemData _newItemData)
    {
        data = _newItemData;
        price = 0;  // ▲ 기본 가격은 0 또는 미정으로 설정
    }

    // 신규 생성자: 아이템 데이터와 가격을 함께 받아 초기화
    public InventoryItem(ItemData _newItemData, int _price)
    {
        data = _newItemData;
        price = _price;
    }
}
