using UnityEngine;
using System.Collections.Generic;

public class ItemAddfd : MonoBehaviour
{
    public Inventory_Item item;
    
    private void Start()
    {
        InventoryManager inventory = InventoryManager.instance;
        inventory.SetItem(SlotType.QuickSlot, inventory.selectedQuickSlot, item);
        InventoryUI.Instance.UpdateAllSlots();
    }
}