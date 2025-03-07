using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EquipmentSlot : UI_ItemSlot
{
    public EquipmentType slotType;
    public int equipmentSlot_Index;

    private void OnValidate()
    {
        gameObject.name = "Equipment slot - " + slotType.ToString();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null)
            return;

        if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Throw Item")))
        {
            PlayerManager.instance.item_drop.EquipmentSlot_Throw(item.data);

            Inventory.instance.UnequipItem(item.data as ItemData_Equipment);
            CleanUpSlot();
            return;
        }

        Inventory.instance.UnequipItem(item.data as ItemData_Equipment);
        Inventory.instance.AddItem(item.data as ItemData_Equipment);

        CleanUpSlot();
    }

    public override void OnDrop(PointerEventData eventData)
    {
        // �巡�׵� �����̳� �������� ������ ����
        if (draggedSlot == null || draggedSlot.item == null)
            return;

        InventoryItem draggedItem = draggedSlot.item;
        UI_QuickSlot draggedQuickSlot = draggedSlot as UI_QuickSlot;

        // �巡�׵� �������� ��� ���������� Ȯ��
        if (draggedItem.data is ItemData_Equipment equipmentData)
        {
            // ��� Ÿ���� ���Կ� �´��� üũ
            if (equipmentData.equipmentType == slotType)
            {
                // ���Կ� �̹� ������ ��� �ִٸ� �����ϰ� �κ��丮�� ��������
                if (item != null && item.data is ItemData_Equipment equippedData)
                {
                    Debug.Log(item.data);
                    Inventory.instance.Move_Item(item.data, draggedSlot.inventorySlotIndex);
                    Inventory.instance.UnequipItem(equippedData);
                    UpdateSlot(draggedItem);
                }

                // �κ��丮���� �ش� ������ ���� �� ���� ó��
                Inventory.instance.EquipItem(equipmentData, draggedSlot.inventorySlotIndex);

                // ��� ���� UI ������Ʈ �� �巡�� ���� ����
                UpdateSlot(draggedItem);
                draggedSlot.CleanUpSlot();
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData) { }

    public override void OnDrag(PointerEventData eventData) { }

    public override void OnEndDrag(PointerEventData eventData) { }
}
