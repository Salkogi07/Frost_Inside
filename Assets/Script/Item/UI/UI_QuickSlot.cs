using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuickSlot : UI_ItemSlot, IDropHandler
{
    public int quickslot_Index; // �ν����Ϳ��� ���� ��ȣ �Ҵ�

    private void OnValidate()
    {
        gameObject.name = "Quick Slot - " + quickslot_Index;
    }

    public override void OnDrop(PointerEventData eventData)
    {
        // �巡�׵� ���԰� �������� ��ȿ���� Ȯ��
        if (draggedSlot == null || draggedSlot.item == null)
            return;

        InventoryItem draggedItem = draggedSlot.item;

        Inventory.instance.RemoveItem(draggedItem.data);

        // UI ������Ʈ: �ش� ���Կ� ������ ������ ǥ��
        UpdateSlot(draggedItem);

        // �ʿ信 ����, �κ��丮 ������ ������ ó���� ���Ѵٸ� �Ʒ� �ּ� ó���� �ڵ带 �����ϼ���.
        // draggedSlot.CleanUpSlot();
    }
}
