using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemImage;
    public InventoryItem item;
    public static UI_ItemSlot draggedSlot;
    public static GameObject draggedItemIcon;

    public int inventorySlotIndex;

    public void UpdateSlot(InventoryItem _newItem)
    {
        item = _newItem;
        itemImage.color = Color.white;

        if (item != null && item.data != null)
        {
            itemImage.sprite = item.data.icon;
        }
        else
        {
            itemImage.sprite = null;
            itemImage.color = Color.clear;
        }
    }

    public void CleanUpSlot()
    {
        item = null;
        itemImage.sprite = null;
        itemImage.color = Color.clear;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (item == null) return;

        if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Throw Item")))
        {
            UI_ItemSlot draggedQuickSlot = draggedSlot;

            PlayerManager.instance.item_drop.Inventory_Throw(item.data, inventorySlotIndex);
            return;
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null || item.data == null) return;
        draggedSlot = this;
        itemImage.raycastTarget = false;

        if (draggedItemIcon == null)
        {
            draggedItemIcon = new GameObject("DraggedItemIcon");
            Image iconImage = draggedItemIcon.AddComponent<Image>();
            iconImage.sprite = itemImage.sprite;
            iconImage.raycastTarget = false;
            draggedItemIcon.transform.SetParent(transform.root);
            draggedItemIcon.transform.SetAsLastSibling();
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (draggedItemIcon != null)
        {
            draggedItemIcon.transform.position = eventData.position;
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        draggedSlot = null;
        itemImage.raycastTarget = true;

        if (draggedItemIcon != null)
        {
            Destroy(draggedItemIcon);
            draggedItemIcon = null;
        }
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this) return;

        // 자기 자신에게 드롭하는 경우 무시
        if (draggedSlot == this)
            return;

        SwapItems(draggedSlot);
    }

    private void SwapItems(UI_ItemSlot otherSlot)
    {
        InventoryItem tempItem = item;
        Inventory.instance.SwapInventoryItems(inventorySlotIndex, otherSlot.inventorySlotIndex);
    }
}
