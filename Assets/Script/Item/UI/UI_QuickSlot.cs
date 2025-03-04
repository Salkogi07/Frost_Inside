using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuickSlot : UI_ItemSlot, IDropHandler
{
    public int slotIndex; // 인스펙터에서 슬롯 번호 할당

    private void OnValidate()
    {
        gameObject.name = "Quick Slot - " + slotIndex;
    }

    public override void OnDrop(PointerEventData eventData)
    {
        // 드래그된 슬롯과 아이템이 유효한지 확인
        if (draggedSlot == null || draggedSlot.item == null)
            return;

        InventoryItem draggedItem = draggedSlot.item;

        Inventory.instance.RemoveItem(draggedItem.data);

        // UI 업데이트: 해당 슬롯에 아이템 아이콘 표시
        UpdateSlot(draggedItem);

        // 필요에 따라, 인벤토리 슬롯의 아이콘 처리를 원한다면 아래 주석 처리된 코드를 참고하세요.
        // draggedSlot.CleanUpSlot();
    }
}
