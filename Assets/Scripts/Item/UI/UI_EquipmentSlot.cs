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

    protected override void ThrowItem()
    {
        PlayerManager.instance.playerDrop
                     .EquipmentSlot_Throw(item.data);

        if (UIManager.instance.itemToolTip.gameObject.activeSelf)
        {
            if (item != null)
                UIManager.instance.itemToolTip.ShowToolTip(item, transform.position);
            else
                UIManager.instance.itemToolTip.HideToolTip();
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null) return;
        Inventory.instance.UnequipItem(item.data as ItemData_Equipment);

        if (Inventory.instance.CanQuickItem())
        {
            Inventory.instance.Add_QuickSlot_Item(item);
            CleanUpSlot();
        }
        else if (Inventory.instance.isPocket)
        {
            Inventory.instance.AddItem(item);
            CleanUpSlot();
        }
        else
        {
            var drop = PlayerManager.instance.playerStats.GetComponent<Player_ItemDrop>();
            drop.Unequipment_ItemDrop(item.data);
            CleanUpSlot();
        }
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot.item == null) return;

        InventoryItem draggedItem = draggedSlot.item;
        UI_QuickSlot draggedQS = draggedSlot as UI_QuickSlot;

        if (draggedItem.data is ItemData_Equipment equipData && equipData.equipmentType == slotType)
        {
            if (item != null && item.data is ItemData_Equipment oldData)
            {
                if (draggedQS != null)
                {
                    Inventory.instance.Move_QuickSlot_Item(item, draggedQS.quickslot_Index);
                    Inventory.instance.UnequipItem(oldData);
                    Inventory.instance.EquipItem(draggedItem.data);
                    UpdateSlot(draggedItem);
                    return;
                }
                else
                {
                    Inventory.instance.Move_Item(item, draggedSlot.inventorySlotIndex);
                    Inventory.instance.UnequipItem(oldData);
                    Inventory.instance.EquipItem(draggedItem.data);
                    UpdateSlot(draggedItem);
                    return;
                }
            }
            else
            {
                if (draggedQS != null)
                {
                    Inventory.instance.EquipItem_ToQuickSlot(draggedItem.data as ItemData_Equipment, draggedQS.quickslot_Index);
                }
                else
                {
                    Inventory.instance.EquipItem_ToInventory(draggedItem.data as ItemData_Equipment, draggedSlot.inventorySlotIndex);
                }
                UpdateSlot(draggedItem);
                draggedSlot.CleanUpSlot();
            }
        }

        if (UIManager.instance.itemToolTip.gameObject.activeSelf)
        {
            if (item != null)
                UIManager.instance.itemToolTip.ShowToolTip(item, transform.position);
            else
                UIManager.instance.itemToolTip.HideToolTip();
        }
    }

    public override void OnBeginDrag(PointerEventData eventData) { }

    public override void OnDrag(PointerEventData eventData) { }

    public override void OnEndDrag(PointerEventData eventData) { }
}
