using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemImage;
    public InventoryItem item;
    public static UI_ItemSlot draggedSlot;
    public static GameObject draggedItemIcon;

    public void UpdateSlot(InventoryItem _newItem)
    {
        item = _newItem;
        itemImage.color = Color.white;

        if (item != null)
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
            PlayerManager.instance.item_drop.GenerateThrow(item.data);
            return;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;
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

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItemIcon != null)
        {
            draggedItemIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
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
        SwapItems(draggedSlot);
    }

    private void SwapItems(UI_ItemSlot otherSlot)
    {
        InventoryItem tempItem = item;
        UpdateSlot(otherSlot.item);
        otherSlot.UpdateSlot(tempItem);
    }
}
