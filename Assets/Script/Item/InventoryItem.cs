using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData data;
    public int price;  // �� ���� �������� ������ ������ �ʵ� �߰�

    // ���� ������: ������ �����͸� �޾� �ʱ�ȭ
    public InventoryItem(ItemData _newItemData)
    {
        data = _newItemData;
        price = 0;  // �� �⺻ ������ 0 �Ǵ� �������� ����
    }

    // �ű� ������: ������ �����Ϳ� ������ �Բ� �޾� �ʱ�ȭ
    public InventoryItem(ItemData _newItemData, int _price)
    {
        data = _newItemData;
        price = _price;
    }
}
