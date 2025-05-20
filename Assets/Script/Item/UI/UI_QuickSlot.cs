using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuickSlot : UI_ItemSlot, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int quickslot_Index; // 인스펙터에서 슬롯 번호 할당

    private void OnValidate()
    {
        gameObject.name = "Quick Slot - " + quickslot_Index;
    }

    protected override void ThrowItem()
    {
        PlayerManager.instance.playerDrop
                     .QuickSlot_Throw(item.data, quickslot_Index);
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot.item == null || draggedSlot == this) return;

        InventoryItem draggedItem = draggedSlot.item;
        UI_QuickSlot origQS = draggedSlot as UI_QuickSlot;

        if (origQS != null)
        {
            Inventory.instance.SwapQuickItems(origQS.quickslot_Index, quickslot_Index);
        }
        else
        {
            if (item != null && item.data != null)
            {
                Inventory.instance.RemoveItem(draggedSlot.inventorySlotIndex);
                Inventory.instance.Move_Item(item, draggedSlot.inventorySlotIndex);
                Inventory.instance.Move_QuickSlot_Item(draggedItem, quickslot_Index);
            }
            else
            {
                Inventory.instance.Move_QuickSlot_Item(draggedItem, quickslot_Index);
                Inventory.instance.RemoveItem(draggedSlot.inventorySlotIndex);
                UpdateSlot(draggedItem);
            }
        }
    }
}
