using UnityEngine.EventSystems;

public class UI_QuickSlot : UI_ItemSlot
{
    protected override void Awake()
    {
        base.Awake();
        
        slotType = SlotType.QuickSlot;
    }
}