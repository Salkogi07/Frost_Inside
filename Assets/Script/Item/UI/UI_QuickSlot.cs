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

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null) return;

        if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Throw Item")))
        {
            PlayerManager.instance.item_drop.QuickSlot_Throw(item.data, quickslot_Index);
            return;
        }
    }

    public override void OnDrop(PointerEventData eventData)
    {
        // 드래그된 슬롯과 아이템이 유효한지 확인
        if (draggedSlot == null || draggedSlot.item == null)
            return;

        // 자기 자신에게 드롭하는 경우 무시
        if (draggedSlot == this)
            return;

        InventoryItem draggedItem = draggedSlot.item;
        UI_QuickSlot draggedQuickSlot = draggedSlot as UI_QuickSlot;

        if (draggedQuickSlot != null)
        {
            // 드래그된 슬롯이 빠른 슬롯인 경우, 두 슬롯의 아이템을 SwapQuickItems 함수를 통해 스왑합니다.
            Inventory.instance.SwapQuickItems(draggedQuickSlot.quickslot_Index, this.quickslot_Index);
        }
        else
        {
            Inventory.instance.Move_QuickSlot_Item(draggedItem.data , this.quickslot_Index);
            Inventory.instance.RemoveItem(draggedItem.data, draggedSlot.inventorySlotIndex);
            UpdateSlot(draggedItem);

            // draggedSlot.CleanUpSlot();
        }
    }
}
