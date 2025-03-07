using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuickSlot : UI_ItemSlot, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int quickslot_Index; // �ν����Ϳ��� ���� ��ȣ �Ҵ�

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
        // �巡�׵� ���԰� �������� ��ȿ���� Ȯ��
        if (draggedSlot == null || draggedSlot.item == null)
            return;

        // �ڱ� �ڽſ��� ����ϴ� ��� ����
        if (draggedSlot == this)
            return;

        InventoryItem draggedItem = draggedSlot.item;
        UI_QuickSlot draggedQuickSlot = draggedSlot as UI_QuickSlot;

        if (draggedQuickSlot != null)
        {
            // �巡�׵� ������ ���� ������ ���, �� ������ �������� SwapQuickItems �Լ��� ���� �����մϴ�.
            Inventory.instance.SwapQuickItems(draggedQuickSlot.quickslot_Index, this.quickslot_Index);
        }
        else
        {
            if(item != null && item.data != null)
            {
                Inventory.instance.RemoveItem(draggedItem.data, draggedSlot.inventorySlotIndex);
                Inventory.instance.Move_Item(item.data, draggedSlot.inventorySlotIndex);
                Inventory.instance.Move_QuickSlot_Item(draggedItem.data, this.quickslot_Index);
            }
            else
            {
                Inventory.instance.Move_QuickSlot_Item(draggedItem.data , this.quickslot_Index);
                Inventory.instance.RemoveItem(draggedItem.data, draggedSlot.inventorySlotIndex);
                UpdateSlot(draggedItem);
            }

            // draggedSlot.CleanUpSlot();
        }
    }
}
