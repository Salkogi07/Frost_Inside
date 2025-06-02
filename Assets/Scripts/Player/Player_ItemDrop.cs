using UnityEngine;
using System.Collections.Generic;

public class Player_ItemDrop : ItemDrop
{
    /// <summary>
    /// 장착 아이템과 인벤토리 아이템을 모두 드롭합니다.
    /// </summary>
    public void GenerateDrop()
    {
        Inventory inventory = Inventory.instance;
        var itemsToUnequip = new List<InventoryItem>();
        var inventoryToDrop = new List<(InventoryItem item, int index)>();
        var quickSlotToDrop = new List<(InventoryItem item, int index)>();

        // 1) 장착 해제 전 드롭
        foreach (InventoryItem eqItem in inventory.GetEquipmentList())
        {
            DropItem(eqItem);
            itemsToUnequip.Add(eqItem);
        }
        // 실제 장비제거 수행
        foreach (var eq in itemsToUnequip)
        {
            inventory.UnequipItem(eq.data as ItemData_Equipment);
        }

        // 2) 인벤토리 아이템 전부 드롭
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
        // 실제 인벤토리에서 제거
        foreach (var pair in inventoryToDrop)
        {
            inventory.RemoveItem(pair.index);
        }

        // 3) 퀵슬롯 아이템 드롭
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
        // 실제 퀵슬롯에서 제거
        foreach (var pair in quickSlotToDrop)
        {
            inventory.Remove_QuickSlot_Item(pair.index);
        }
    }

    /// <summary>
    /// 장착 슬롯에서 언이큅만 드롭
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
    /// 포켓(인벤토리 슬롯)에서 드롭
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
    /// 인벤토리 슬롯에서 던지기
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
    /// 장착 슬롯에서 던지기
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
    /// 퀵슬롯에서 던지기
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
