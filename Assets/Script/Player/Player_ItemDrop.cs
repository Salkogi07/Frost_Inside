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
            inventory.RemoveItem(inventoryToDrop[i].data);
        }
    }

    public void GenerateThrow(ItemData _itemdata)
    {
        ThrowItem(_itemdata);
        Inventory.instance.RemoveItem(_itemdata);
    }
}
