using UnityEngine;

public class UI_EquipmentSlot : UI_ItemSlot
{
    [Tooltip("이 슬롯이 담당할 장비의 종류를 설정합니다.")]
    public EquipmentType equipmentType;
    
    protected override void Awake()
    {
        base.Awake();
        
        slotType = SlotType.Equipment;
    }
    
    private void OnValidate()
    {
        if (equipmentType != null)
        {
            gameObject.name = "Equipment Slot - " + equipmentType.ToString();
        }
    }
}