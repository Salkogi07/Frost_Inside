using System;
using UnityEngine;

[Serializable]
public class Inventory_Item
{
    public ItemData data;
    public int price;
    
    public Inventory_Item(ItemData _newItemData)
    {
        data = _newItemData;
        price = 0;
    }
    
    public Inventory_Item(ItemData _newItemData, int _price)
    {
        data = _newItemData;
        price = _price;
    }
}
