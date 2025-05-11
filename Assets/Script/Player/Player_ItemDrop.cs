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

        // 1) 장착 해제 전 드롭
        foreach (InventoryItem eqItem in inventory.GetEquipmentList())
        {
            DropItem(eqItem);  // InventoryItem 자체를 넘김 :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}
            itemsToUnequip.Add(eqItem);
        }
        // 실제 언이큅 수행
        foreach (var eq in itemsToUnequip)
        {
            inventory.UnequipItem(eq.data as ItemData_Equipment);
        }

        // 2) 인벤토리 아이템 전부 드롭
        InventoryItem[] invList = inventory.GetInventoryList();  // :contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}
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
    }

    /// <summary>
    /// 장착 슬롯에서 언이큅만 드롭
    /// </summary>
    public void Unequipment_ItemDrop(ItemData _itemData)
    {
        var eqList = Inventory.instance.GetEquipmentList();  // :contentReference[oaicite:4]{index=4}:contentReference[oaicite:5]{index=5}
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
        InventoryItem item = Inventory.instance.GetInventoryList()[index];  // :contentReference[oaicite:6]{index=6}:contentReference[oaicite:7]{index=7}
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
        InventoryItem item = Inventory.instance.GetInventoryList()[index];  // :contentReference[oaicite:8]{index=8}:contentReference[oaicite:9]{index=9}
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
        var eqList = Inventory.instance.GetEquipmentList();  // :contentReference[oaicite:10]{index=10}:contentReference[oaicite:11]{index=11}
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
        InventoryItem item = Inventory.instance.quickSlotItems[index];  // :contentReference[oaicite:12]{index=12}:contentReference[oaicite:13]{index=13}
        if (item != null)
        {
            ThrowItem(item);
            Inventory.instance.Remove_QuickSlot_Item(index);
        }
    }
}
