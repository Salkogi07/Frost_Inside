using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EquipmentSlot : UI_ItemSlot
{
    public EquipmentType slotType;

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
            PlayerManager.instance.item_drop.GenerateThrow(item.data);
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

        // �巡�׵� �������� ��� ���������� Ȯ��
        if (draggedItem.data is ItemData_Equipment equipmentData)
        {
            // ��� Ÿ���� ���Կ� �´��� üũ
            if (equipmentData.equipmentType == slotType)
            {
                // ���Կ� �̹� ������ ��� �ִٸ� �����ϰ� �κ��丮�� ��������
                if (item != null && item.data is ItemData_Equipment equippedData)
                {
                    Inventory.instance.UnequipItem(equippedData);
                }

                // �κ��丮���� �ش� ������ ���� �� ���� ó��
                Inventory.instance.EquipItem(equipmentData);

                // ��� ���� UI ������Ʈ �� �巡�� ���� ����
                UpdateSlot(draggedItem);
                draggedSlot.CleanUpSlot();
            }
        }
    }
}
