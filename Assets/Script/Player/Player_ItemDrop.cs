using UnityEngine;
using System.Collections.Generic;

public class Player_ItemDrop : ItemDrop
{
    public void GenerateDrop()
    {
        Inventory inventory = Inventory.instance;

        List<InventoryItem> itemsToUnequip = new List<InventoryItem>();
        List<InventoryItem> inventoryToDrop = new List<InventoryItem>();

        foreach (InventoryItem item in inventory.GetEquipmentList())
        {
            DropItem(item.data);
            itemsToUnequip.Add(item);
        }

        for(int i = 0; i < itemsToUnequip.Count; i++)
        {
            inventory.UnequipItem(itemsToUnequip[i].data as ItemData_Equipment);
        }


        foreach (InventoryItem item in inventory.GetInventoryList())
        {
            DropItem(item.data);
            inventoryToDrop.Add(item);
        }

        for(int i = 0;i < inventoryToDrop.Count; i++)
        {
            inventory.RemoveItem(inventoryToDrop[i].data, i);
        }
    }

    public void Inventory_Throw(ItemData _itemdata, int index)
    {
        ThrowItem(_itemdata);
        Inventory.instance.RemoveItem(_itemdata, index);
    }

    public void EquipmentSlot_Throw(ItemData _itemdata)
    {
        ThrowItem(_itemdata);
        Inventory.instance.UnequipItem(_itemdata as ItemData_Equipment);
    }

    public void QuickSlot_Throw(ItemData _itemdata, int index)
    {
        ThrowItem(_itemdata);
        Inventory.instance.Remove_QuickSlot_Item(_itemdata, index);
    }
}
