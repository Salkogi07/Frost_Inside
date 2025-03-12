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

        if (Inventory.instance.CanQuickItem())
        {
            Inventory.instance.UnequipItem(item.data as ItemData_Equipment);
            Inventory.instance.Add_QuickSlot_Item(item.data as ItemData_Equipment);

            CleanUpSlot();
        }
        else
        {
            if (Inventory.instance.isPocket)
            {
                Inventory.instance.UnequipItem(item.data as ItemData_Equipment);
                Inventory.instance.AddItem(item.data as ItemData_Equipment);

                CleanUpSlot();
            }
            else
            {
                Inventory.instance.UnequipItem(item.data as ItemData_Equipment);
                Player_ItemDrop itemDrop = PlayerManager.instance.player.GetComponent<Player_ItemDrop>();
                itemDrop.Unequipment_ItemDrop(item.data);
                CleanUpSlot();
            }
        }
    }

    public override void OnDrop(PointerEventData eventData)
    {
        // 드래그된 슬롯이나 아이템이 없으면 무시
        if (draggedSlot == null || draggedSlot.item == null)
            return;

        InventoryItem draggedItem = draggedSlot.item;
        UI_QuickSlot draggedQuickSlot = draggedSlot as UI_QuickSlot;

        // 드래그된 아이템이 장비 아이템인지 확인
        if (draggedItem.data is ItemData_Equipment equipmentData)
        {
            // 장비 타입이 슬롯에 맞는지 체크
            if (equipmentData.equipmentType == slotType)
            {
                // 슬롯에 이미 장착된 장비가 있다면 해제하고 인벤토리로 돌려보냄
                if (item != null && item.data is ItemData_Equipment equippedData)
                {
                    if(draggedQuickSlot != null)
                    {
                        Inventory.instance.Move_QuickSlot_Item(item.data, draggedQuickSlot.quickslot_Index);
                        Inventory.instance.UnequipItem(equippedData);
                        Inventory.instance.EquipItem(draggedItem.data);
                        UpdateSlot(draggedItem);
                        return;
                    }
                    else
                    {
                        Inventory.instance.Move_Item(item.data, draggedSlot.inventorySlotIndex);
                        Inventory.instance.UnequipItem(equippedData);
                        Inventory.instance.EquipItem(draggedItem.data);
                        UpdateSlot(draggedItem);
                        return;
                    }
                }
                else
                {
                    if(draggedQuickSlot != null)
                    {
                        // 인벤토리에서 해당 아이템 제거 후 장착 처리
                        Inventory.instance.EquipItem_ToQuickSlot(equipmentData, draggedQuickSlot.quickslot_Index);

                        // 장비 슬롯 UI 업데이트 및 드래그 슬롯 정리
                        UpdateSlot(draggedItem);
                        draggedSlot.CleanUpSlot();
                    }
                    else
                    {
                        // 인벤토리에서 해당 아이템 제거 후 장착 처리
                        Inventory.instance.EquipItem_ToInventory(equipmentData, draggedSlot.inventorySlotIndex);

                        // 장비 슬롯 UI 업데이트 및 드래그 슬롯 정리
                        UpdateSlot(draggedItem);
                        draggedSlot.CleanUpSlot();
                    }
                }       
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData) { }

    public override void OnDrag(PointerEventData eventData) { }

    public override void OnEndDrag(PointerEventData eventData) { }
}
