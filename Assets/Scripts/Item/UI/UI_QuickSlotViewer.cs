using UnityEngine;
using UnityEngine.EventSystems;

public class UI_QuickSlotViewer : UI_ItemSlot
{
    [SerializeField] private GameObject highlight;
    
    protected override void Awake()
    {
        base.Awake();
        
        slotType = SlotType.QuickSlotViewer;
    }
    
    public void SetHighlight(bool state)
    {
        if (highlight != null)
        {
            highlight.SetActive(state);
        }
    }
    
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (item.IsEmpty()) return;

        draggedSlot = this;
        itemImage.raycastTarget = false;
    }
}