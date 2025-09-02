using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SlotType { Poket, QuickSlot, Equipment, QuickSlotViewer }

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Image itemImage;
    
    protected Inventory_Item item;
    
    public SlotType slotType = SlotType.Poket;
    public int slotIndex;

    public static UI_ItemSlot draggedSlot;
    private static GameObject draggedItemIcon;
    private Canvas rootCanvas;

    protected virtual void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
    }
    
    public void UpdateSlot(Inventory_Item _newItem)
    {
        item = _newItem;
        itemImage.color = Color.white;

        if (!item.IsEmpty() && item.Data != null)
        {
            itemImage.sprite = item.Data.icon;
        }
        else
        {
            itemImage.sprite = null;
            itemImage.color = new Color(1, 1, 1, 0); // 투명하게
        }
    }

    public void CleanUpSlot()
    {
        item = Inventory_Item.Empty;
        itemImage.sprite = null;
        itemImage.color = new Color(1, 1, 1, 0);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (item.IsEmpty()) return;

        draggedSlot = this;
        itemImage.raycastTarget = false;

        // 드래그 아이콘 생성
        draggedItemIcon = new GameObject("DraggedIcon");
        draggedItemIcon.transform.SetParent(rootCanvas.transform);
        draggedItemIcon.transform.SetAsLastSibling();
        draggedItemIcon.transform.localScale = Vector3.one;

        Image newImage = draggedItemIcon.AddComponent<Image>();
        newImage.sprite = item.Data.icon;
        newImage.raycastTarget = false;
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
        itemImage.raycastTarget = true;
        if (draggedItemIcon != null)
        {
            Destroy(draggedItemIcon);
        }
        draggedSlot = null;
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this) return;
        
        // InventoryManager에 아이템 교환 요청
        InventoryManager.Instance.SwapItems(draggedSlot.slotType, draggedSlot.slotIndex, this.slotType, this.slotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item.IsEmpty()) return;
        // UIManager.instance.itemToolTip.ShowToolTip(item, transform.position); // 툴팁 표시
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // UIManager.instance.itemToolTip.HideToolTip(); // 툴팁 숨기기
    }
    
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // 필요시 우클릭 등 이벤트 처리
    }
}