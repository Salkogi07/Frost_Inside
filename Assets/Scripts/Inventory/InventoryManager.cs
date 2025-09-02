using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Data")]
    public List<Inventory_Item> startingItems;
    public Inventory_Item[] inventoryItems;
    public Inventory_Item[] quickSlotItems;
    public Inventory_Item[] equipmentItems;

    [Header("State")]
    public bool isPocket = false;
    public bool isInvenOpen = false;
    public int selectedQuickSlot = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 장비 타입의 개수만큼 배열 초기화
        int equipmentTypeCount = System.Enum.GetNames(typeof(EquipmentType)).Length;
        equipmentItems = new Inventory_Item[equipmentTypeCount];
        for (int i = 0; i < equipmentItems.Length; i++)
        {
            equipmentItems[i] = Inventory_Item.Empty;
        }

        // 인벤토리와 퀵슬롯은 UI에서 크기를 받아와야 하므로 InventoryUI에서 초기화
    }
    
    public void InitializeSlots(int inventorySize, int quickSlotSize)
    {
        inventoryItems = new Inventory_Item[inventorySize];
        quickSlotItems = new Inventory_Item[quickSlotSize];

        for (int i = 0; i < inventoryItems.Length; i++) inventoryItems[i] = Inventory_Item.Empty;
        for (int i = 0; i < quickSlotItems.Length; i++) quickSlotItems[i] = Inventory_Item.Empty;
        
        AddStartingItems();
        InventoryUI.Instance.UpdateAllSlots();
    }


    private void Update()
    {
        if (!isPocket)
        {
            // NotPocket_ItemDrop(); // PlayerDrop 관련 로직으로 이동 필요
        }

        UpdateInventory();
        HandleQuickSlotSelection();
        HandleItemThrowInput();
    }

    private void UpdateInventory()
    {
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Open Inventory")))
        {
            isInvenOpen = !isInvenOpen;
            InventoryUI.Instance.UpdateInventoryPanel();
        }
    }

    private void HandleQuickSlotSelection()
    {
        if (isInvenOpen || SettingManager.Instance.IsOpenSetting()) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectQuickSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectQuickSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectQuickSlot(2);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0) SelectQuickSlot((selectedQuickSlot + 1) % quickSlotItems.Length);
        if (scroll > 0) SelectQuickSlot((selectedQuickSlot - 1 + quickSlotItems.Length) % quickSlotItems.Length);
    }

    private void HandleItemThrowInput()
    {
         if (isInvenOpen || SettingManager.Instance.IsOpenSetting()) return;
         
        if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Throw Item")))
        {
            if (!quickSlotItems[selectedQuickSlot].IsEmpty())
            {
                // PlayerManager.instance.playerDrop.QuickSlot_Throw(quickSlotItems[selectedQuickSlot].Data, selectedQuickSlot);
                // 아이템을 버린 후에는 해당 슬롯을 비워야 합니다.
                // quickSlotItems[selectedQuickSlot] = Inventory_Item.Empty;
                // InventoryUI.Instance.UpdateAllSlots();
            }
        }
    }

    private void AddStartingItems()
    {
        foreach (var item in startingItems)
        {
            AddItem(item);
        }
    }

    public void SelectQuickSlot(int index)
    {
        if (index < 0 || index >= quickSlotItems.Length) return;
        selectedQuickSlot = index;
        InventoryUI.Instance.UpdateAllSlots(); // 선택 UI 변경
    }

    #region Item Management

    public bool AddItem(Inventory_Item itemToAdd)
    {
        int emptySlotIndex = FindFirstEmptySlot(inventoryItems);
        if (emptySlotIndex != -1)
        {
            inventoryItems[emptySlotIndex] = itemToAdd;
            InventoryUI.Instance.UpdateAllSlots();
            return true;
        }
        return false;
    }
    
    public void SwapItems(SlotType sourceType, int sourceIndex, SlotType targetType, int targetIndex)
    {
        Inventory_Item sourceItem = GetItem(sourceType, sourceIndex);
        Inventory_Item targetItem = GetItem(targetType, targetIndex);

        // 장비 슬롯으로 아이템을 옮기는 경우, 타입 검사
        if (targetType == SlotType.Equipment)
        {
            if (sourceItem.Data is ItemData_Equipment equipData)
            {
                if ((int)equipData.equipmentType != targetIndex) // 장비 타입과 슬롯 인덱스가 일치하지 않으면 return
                {
                    Debug.Log("이 슬롯에 장착할 수 없는 아이템입니다.");
                    return; 
                }
            }
            else // 장비 아이템이 아니면 장착 불가
            {
                Debug.Log("장비 아이템만 장착할 수 있습니다.");
                return;
            }
        }
        
        // 장비 슬롯에서 아이템을 빼는 경우도 타입 검사 (다른 장비 슬롯으로 옮길 때)
        if (sourceType == SlotType.Equipment && targetType == SlotType.Equipment)
        {
            if (targetItem.Data is ItemData_Equipment targetEquipData)
            {
                 if ((int)targetEquipData.equipmentType != sourceIndex) return;
            }
        }
        
        // 실제 아이템 교환
        SetItem(sourceType, sourceIndex, targetItem);
        SetItem(targetType, targetIndex, sourceItem);

        // 장비 아이템 효과 적용/해제
        HandleEquipmentEffects(sourceItem, targetType);
        HandleEquipmentEffects(targetItem, sourceType);

        InventoryUI.Instance.UpdateAllSlots();
    }

    private void HandleEquipmentEffects(Inventory_Item item, SlotType slotType)
    {
        if (item.Data is ItemData_Equipment equipData)
        {
            if (slotType == SlotType.Equipment)
            {
                equipData.AddModifiers();
                equipData.ExecuteItemEffect();
            }
            else
            {
                equipData.RemoveModifiers();
                equipData.UnExecuteItemEffect();
            }
        }
    }


    public Inventory_Item GetItem(SlotType slotType, int index)
    {
        switch (slotType)
        {
            case SlotType.Inventory: return inventoryItems[index];
            case SlotType.QuickSlot: return quickSlotItems[index];
            case SlotType.Equipment: return equipmentItems[index];
            default: return Inventory_Item.Empty;
        }
    }
    
    public void SetItem(SlotType slotType, int index, Inventory_Item item)
    {
        switch (slotType)
        {
            case SlotType.Inventory: inventoryItems[index] = item; break;
            case SlotType.QuickSlot: quickSlotItems[index] = item; break;
            case SlotType.Equipment: equipmentItems[index] = item; break;
        }
    }


    private int FindFirstEmptySlot(Inventory_Item[] itemArray)
    {
        for (int i = 0; i < itemArray.Length; i++)
        {
            if (itemArray[i].IsEmpty())
            {
                return i;
            }
        }
        return -1;
    }
    
    public bool CanAddItem() => FindFirstEmptySlot(inventoryItems) != -1;
    public bool CanQuickItem() => FindFirstEmptySlot(quickSlotItems) != -1;

    #endregion
    
    #region Equipment Specific
    public ItemData_Equipment GetEquipment(EquipmentType _type)
    {
        return equipmentItems[(int)_type].Data as ItemData_Equipment;
    }
    
    public List<Inventory_Item> GetEquipmentList()
    {
        return equipmentItems.Where(item => !item.IsEmpty()).ToList();
    }
    #endregion
}