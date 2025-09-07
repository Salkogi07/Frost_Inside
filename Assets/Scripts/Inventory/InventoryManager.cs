using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [Header("Data")]
    public List<Inventory_Item> startingItems;
    public Inventory_Item[] inventoryItems;
    public Inventory_Item[] quickSlotItems;
    public Inventory_Item[] equipmentItems;

    [Header("State")]
    public bool isPocket = false;
    public bool isInvenOpen = false;
    public int selectedQuickSlot = 0;
    
    private bool hasBeenInitialized = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //시작 아이템 지급
        AddStartingItems();
    }
    
    public void InitializeSlots(int inventorySize, int quickSlotSize)
    {
        if (hasBeenInitialized)
        {
            // 새 씬의 UI가 기존 아이템 정보를 표시하도록 업데이트만 호출합니다.
            InventoryUI.Instance.UpdateAllSlots();
            return;
        }
        
        int equipmentTypeCount = System.Enum.GetNames(typeof(EquipmentType)).Length;
        equipmentItems = new Inventory_Item[equipmentTypeCount];
        inventoryItems = new Inventory_Item[inventorySize];
        quickSlotItems = new Inventory_Item[quickSlotSize];
        
        for (int i = 0; i < equipmentItems.Length; i++) equipmentItems[i] = Inventory_Item.Empty;
        for (int i = 0; i < inventoryItems.Length; i++) inventoryItems[i] = Inventory_Item.Empty;
        for (int i = 0; i < quickSlotItems.Length; i++) quickSlotItems[i] = Inventory_Item.Empty;
        
        hasBeenInitialized = true;
        
        InventoryUI.Instance.UpdateAllSlots();
    }


    private void Update()
    {
        UpdateInventory();
        HandleQuickSlotSelection();
        HandleItemThrowInput();
    }
    
    public void UseItemInQuickSlot()
    {
        // 선택된 퀵슬롯의 아이템을 가져옵니다.
        Inventory_Item itemToUse = quickSlotItems[selectedQuickSlot];

        // 아이템이 비어있지 않고, 사용 가능한 아이템(ItemData_UseItem)인지 확인합니다.
        if (!itemToUse.IsEmpty() && itemToUse.Data.itemType == ItemType.UseItem)
        {
            if (itemToUse.Data is ItemData_UseItem useItemData)
            {
                useItemData.ExecuteItemEffect(GameManager.instance.playerPrefab.transform);
                quickSlotItems[selectedQuickSlot] = Inventory_Item.Empty;

                InventoryUI.Instance.UpdateAllSlots();
            }
        }
    }

    private void HandleItemThrowInput()
    {
        if (isInvenOpen || SettingManager.Instance.IsOpenSetting()) return;
        
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Throw Item")))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.playerPrefab.GetComponent<Player_ItemDrop>().DropItem(SlotType.QuickSlot, selectedQuickSlot);
            }
        }
    }

    private void UpdateInventory()
    {
        if (SettingManager.Instance.IsOpenSetting()) return;
        
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Open Inventory")))
        {
            isInvenOpen = !isInvenOpen;
            
            if (!isInvenOpen)
            {
                UI_ItemSlot.CancelDrag();
            }
            
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
        if (!targetItem.IsEmpty() && sourceType == SlotType.Equipment)
        {
            if (targetItem.Data is ItemData_Equipment targetEquipData)
            {
                // 장비 타입이 시작 슬롯의 인덱스와 일치하는지 확인
                if ((int)targetEquipData.equipmentType != sourceIndex)
                {
                    Debug.Log("이 슬롯에 장착할 수 없는 종류의 장비입니다.");
                    return;
                }
            }
            else // 장비 아이템이 아니면 장비 슬롯으로 들어올 수 없음
            {
                Debug.Log("장비가 아닌 아이템과 교체할 수 없습니다.");
                return;
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
            case SlotType.Poket: return inventoryItems[index];
            case SlotType.QuickSlot: return quickSlotItems[index];
            case SlotType.Equipment: return equipmentItems[index];
            default: return Inventory_Item.Empty;
        }
    }
    
    public void SetItem(SlotType slotType, int index, Inventory_Item item)
    {
        switch (slotType)
        {
            case SlotType.Poket: inventoryItems[index] = item; break;
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

    public void UpdatePlayerWeight()
    {
        GameManager.instance.playerPrefab.GetComponent<Player_Condition>().Weight = CalculateTotalWeight();
    }
    
    private float CalculateTotalWeight()
    {
        float totalWeight = 0;

        totalWeight += inventoryItems.Where(item => !item.IsEmpty()).Sum(item => item.Data.itemWeight);
        totalWeight += quickSlotItems.Where(item => !item.IsEmpty()).Sum(item => item.Data.itemWeight);
        totalWeight += equipmentItems.Where(item => !item.IsEmpty()).Sum(item => item.Data.itemWeight);

        return totalWeight;
    }
}