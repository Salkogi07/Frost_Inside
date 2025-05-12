using UnityEngine;
using System.Collections.Generic;

public class Player_ItemDrop : ItemDrop
{
    /// <summary>
    /// ���� �����۰� �κ��丮 �������� ��� ����մϴ�.
    /// </summary>
    public void GenerateDrop()
    {
        Inventory inventory = Inventory.instance;
        var itemsToUnequip = new List<InventoryItem>();
        var inventoryToDrop = new List<(InventoryItem item, int index)>();
        var quickSlotToDrop = new List<(InventoryItem item, int index)>();

        // 1) ���� ���� �� ���
        foreach (InventoryItem eqItem in inventory.GetEquipmentList())
        {
            DropItem(eqItem);
            itemsToUnequip.Add(eqItem);
        }
        // ���� ������� ����
        foreach (var eq in itemsToUnequip)
        {
            inventory.UnequipItem(eq.data as ItemData_Equipment);
        }

        // 2) �κ��丮 ������ ���� ���
        InventoryItem[] invList = inventory.GetInventoryList();
        for (int i = 0; i < invList.Length; i++)
        {
            InventoryItem invItem = invList[i];
            if (invItem?.data != null)
            {
                DropItem(invItem);
                inventoryToDrop.Add((invItem, i));
            }
        }
        // ���� �κ��丮���� ����
        foreach (var pair in inventoryToDrop)
        {
            inventory.RemoveItem(pair.index);
        }

        // 3) ������ ������ ���
        InventoryItem[] quickSlotItems = inventory.quickSlotItems;
        for (int i = 0; i < quickSlotItems.Length; i++)
        {
            InventoryItem quickItem = quickSlotItems[i];
            if (quickItem?.data != null)
            {
                DropItem(quickItem);
                quickSlotToDrop.Add((quickItem, i));
            }
        }
        // ���� �����Կ��� ����
        foreach (var pair in quickSlotToDrop)
        {
            inventory.Remove_QuickSlot_Item(pair.index);
        }
    }

    /// <summary>
    /// ���� ���Կ��� ����Ţ�� ���
    /// </summary>
    public void Unequipment_ItemDrop(ItemData _itemData)
    {
        var eqList = Inventory.instance.GetEquipmentList();
        InventoryItem item = eqList.Find(x => x.data == _itemData);
        if (item != null)
        {
            DropItem(item);
        }
    }

    /// <summary>
    /// ����(�κ��丮 ����)���� ���
    /// </summary>
    public void Pocket_Inventory_Drop(ItemData _itemData, int index)
    {
        InventoryItem item = Inventory.instance.GetInventoryList()[index];
        if (item != null)
        {
            DropItem(item);
            Inventory.instance.RemoveItem(index);
        }
    }

    /// <summary>
    /// �κ��丮 ���Կ��� ������
    /// </summary>
    public void Inventory_Throw(ItemData _itemData, int index)
    {
        InventoryItem item = Inventory.instance.GetInventoryList()[index];
        if (item != null)
        {
            ThrowItem(item);
            Inventory.instance.RemoveItem(index);
        }
    }

    /// <summary>
    /// ���� ���Կ��� ������
    /// </summary>
    public void EquipmentSlot_Throw(ItemData _itemData)
    {
        var eqList = Inventory.instance.GetEquipmentList();
        InventoryItem item = eqList.Find(x => x.data == _itemData);
        if (item != null)
        {
            ThrowItem(item);
            Inventory.instance.UnequipItem(_itemData as ItemData_Equipment);
        }
    }

    /// <summary>
    /// �����Կ��� ������
    /// </summary>
    public void QuickSlot_Throw(ItemData _itemData, int index)
    {
        InventoryItem item = Inventory.instance.quickSlotItems[index];
        if (item != null)
        {
            ThrowItem(item);
            Inventory.instance.Remove_QuickSlot_Item(index);
        }
    }
}
