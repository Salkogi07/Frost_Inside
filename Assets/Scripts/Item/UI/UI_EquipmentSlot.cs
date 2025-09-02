using UnityEngine;

/// <summary>
/// 장비 아이템을 표시하는 UI 슬롯입니다.
/// UI_ItemSlot을 상속받아 장비 슬롯에 특화된 기능을 정의합니다.
/// </summary>
public class UI_EquipmentSlot : UI_ItemSlot
{
    [Tooltip("이 슬롯이 담당할 장비의 종류를 설정합니다.")]
    public EquipmentType equipmentType;
    
    protected override void Awake()
    {
        base.Awake();
        
        slotType = SlotType.Equipment;
        
        slotIndex = (int)equipmentType;
    }
    
    private void OnValidate()
    {
        if (equipmentType != null)
        {
            gameObject.name = "Equipment Slot - " + equipmentType.ToString();
        }
    }
}