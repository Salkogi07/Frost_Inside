using UnityEngine;
using System.Collections.Generic;

public class Player_ItemDrop : ItemDrop
{
    public void GenerateDrop()
    {
        Inventory inventory = Inventory.instance;
        var itemsToUnequip = new List<Inventory_Item>();
        var inventoryToDrop = new List<(Inventory_Item item, int index)>();
        var quickSlotToDrop = new List<(Inventory_Item item, int index)>();
        
        foreach (Inventory_Item eqItem in inventory.GetEquipmentList())
        {
            DropItem(eqItem);
            itemsToUnequip.Add(eqItem);
        }
        foreach (var eq in itemsToUnequip)
        {
            inventory.UnequipItem(eq.data as ItemData_Equipment);
        }
        
        Inventory_Item[] invList = inventory.GetInventoryList();
        for (int i = 0; i < invList.Length; i++)
        {
            Inventory_Item invItem = invList[i];
            if (invItem?.data != null)
            {
                DropItem(invItem);
                inventoryToDrop.Add((invItem, i));
            }
        }
        foreach (var pair in inventoryToDrop)
        {
            inventory.RemoveItem(pair.index);
        }
        
        Inventory_Item[] quickSlotItems = inventory.quickSlotItems;
        for (int i = 0; i < quickSlotItems.Length; i++)
        {
            Inventory_Item quickItem = quickSlotItems[i];
            if (quickItem?.data != null)
            {
                DropItem(quickItem);
                quickSlotToDrop.Add((quickItem, i));
            }
        }
        foreach (var pair in quickSlotToDrop)
        {
            inventory.Remove_QuickSlot_Item(pair.index);
        }
    }
    
    public void Unequipment_ItemDrop(ItemData _itemData)
    {
        var eqList = Inventory.instance.GetEquipmentList();
        Inventory_Item item = eqList.Find(x => x.data == _itemData);
        if (item != null)
        {
            DropItem(item);
        }
    }
    
    public void Pocket_Inventory_Drop(ItemData _itemData, int index)
    {
        Inventory_Item item = Inventory.instance.GetInventoryList()[index];
        if (item != null)
        {
            DropItem(item);
            Inventory.instance.RemoveItem(index);
        }
    }
    
    public void Inventory_Throw(ItemData _itemData, int index)
    {
        Inventory_Item item = Inventory.instance.GetInventoryList()[index];
        if (item != null)
        {
            ThrowItem(item);
            Inventory.instance.RemoveItem(index);
        }
    }
    
    public void EquipmentSlot_Throw(ItemData _itemData)
    {
        var eqList = Inventory.instance.GetEquipmentList();
        Inventory_Item item = eqList.Find(x => x.data == _itemData);
        if (item != null)
        {
            ThrowItem(item);
            Inventory.instance.UnequipItem(_itemData as ItemData_Equipment);
        }
    }
    
    public void QuickSlot_Throw(ItemData _itemData, int index)
    {
        Inventory_Item item = Inventory.instance.quickSlotItems[index];
        if (item != null)
        {
            ThrowItem(item);
            Inventory.instance.Remove_QuickSlot_Item(index);
        }
    }
}
