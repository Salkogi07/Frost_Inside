using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public bool isPocket = false;
    public bool isInvenOpen = false;

    public List<InventoryItem> startingItems;

    public InventoryItem[] quickSlotItems;
    public int selectedQuickSlot = 0;

    public List<InventoryItem> equipment;
    public Dictionary<ItemData_Equipment, InventoryItem> equipmentDictionary;

    public InventoryItem[] inventoryItems;

    [Header("Inventory UI")]
    [SerializeField] private Transform inventorySlotParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform quickSlotParent;

    private UI_ItemSlot[] inventoryItemSlot;
    private UI_EquipmentSlot[] equipmentSlot;
    private UI_QuickSlot[] quickSlot;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(instance);
    }

    private void Start()
    {
        equipment = new List<InventoryItem>();
        equipmentDictionary = new Dictionary<ItemData_Equipment, InventoryItem>();

        inventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();
        quickSlot = quickSlotParent.GetComponentsInChildren<UI_QuickSlot>();

        inventoryItems = new InventoryItem[inventoryItemSlot.Length];
        quickSlotItems = new InventoryItem[quickSlot.Length];

        AddStartingItems();
    }

    private void Update()
    {
        if (!GameManager.instance.isSetting)
            return;

        if (!isPocket)
        {
            NotPocket_ItemDrop();
        }

        if (!isInvenOpen && !SettingManager.Instance.IsOpenSetting())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectQuickSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectQuickSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectQuickSlot(2);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll < 0) SelectQuickSlot((selectedQuickSlot + 1) % quickSlot.Length);
            if (scroll > 0) SelectQuickSlot((selectedQuickSlot + 2) % quickSlot.Length);

            if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Throw Item")))
            {
                if(quickSlotItems[selectedQuickSlot]?.data != null)
                {
                    PlayerManager.instance.playerDrop.QuickSlot_Throw(quickSlotItems[selectedQuickSlot].data, selectedQuickSlot);
                    return;
                }
            }
        }
    }

    private void NotPocket_ItemDrop()
    {
        Player_ItemDrop itemDrop = PlayerManager.instance.playerStats.GetComponent<Player_ItemDrop>();
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i]?.data != null)
            {
                itemDrop.Pocket_Inventory_Drop(inventoryItems[i].data, i);
                inventoryItems[i] = null;
            }
        }
    }

    private void AddStartingItems()
    {
        foreach (var item in startingItems)
            AddItem(item);
    }

    public void EquipItem(ItemData _item)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
        newEquipment.ExecuteItemEffect();
        InventoryItem newItem = new InventoryItem(newEquipment);

        ItemData_Equipment oldEquipment = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
        {
            if (item.Key.equipmentType == newEquipment.equipmentType)
                oldEquipment = item.Key;
        }

        if (oldEquipment != null)
        {
            UnequipItem(oldEquipment);
            AddItem(new InventoryItem(oldEquipment));
        }

        equipment.Add(newItem);
        equipmentDictionary.Add(newEquipment, newItem);
        newEquipment.AddModifiers();

        UpdateSlotUI();
    }

    public void EquipItem_ToInventory(ItemData _item, int index)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
        newEquipment.ExecuteItemEffect();
        InventoryItem newItem = new InventoryItem(newEquipment);

        ItemData_Equipment oldEquipment = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
        {
            if (item.Key.equipmentType == newEquipment.equipmentType)
                oldEquipment = item.Key;
        }

        if (oldEquipment != null)
        {
            UnequipItem(oldEquipment);
            AddItem(new InventoryItem(oldEquipment));
        }

        equipment.Add(newItem);
        equipmentDictionary.Add(newEquipment, newItem);
        newEquipment.AddModifiers();

        RemoveItem(index);

        UpdateSlotUI();
    }

    public void EquipItem_ToQuickSlot(ItemData _item, int index)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
        InventoryItem newItem = new InventoryItem(newEquipment);
        newEquipment?.ExecuteItemEffect();

        ItemData_Equipment oldEquipment = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
        {
            if (item.Key.equipmentType == newEquipment.equipmentType)
                oldEquipment = item.Key;
        }

        if (oldEquipment != null)
        {
            UnequipItem(oldEquipment);
            Add_QuickSlot_Item(new InventoryItem(oldEquipment));
        }

        equipment.Add(newItem);
        equipmentDictionary.Add(newEquipment, newItem);
        newEquipment.AddModifiers();

        Remove_QuickSlot_Item(index);

        UpdateSlotUI();
    }

    public void UnequipItem(ItemData_Equipment itemToRemove)
    {
        if (equipmentDictionary.TryGetValue(itemToRemove, out InventoryItem newItem))
        {
            equipment.Remove(newItem);
            equipmentDictionary.Remove(itemToRemove);
            itemToRemove.RemoveModifiers();
            itemToRemove?.UnExecuteItemEffect();
        }
    }

    private void UpdateSlotUI()
    {
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
            {
                if (item.Key.equipmentType == equipmentSlot[i].slotType)
                    equipmentSlot[i].UpdateSlot(item.Value);
            }
        }

        for (int i = 0; i < inventoryItemSlot.Length; i++)
        {
            inventoryItemSlot[i].CleanUpSlot();
        }
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            inventoryItemSlot[i].UpdateSlot(inventoryItems[i]);
        }

        for (int i = 0; i < quickSlot.Length; i++)
        {
            quickSlot[i].CleanUpSlot();
        }
        for (int i = 0; i < quickSlotItems.Length; i++)
        {
            if (i < quickSlot.Length)
                quickSlot[i].UpdateSlot(quickSlotItems[i]);
        }
    }

    public void AddItem(InventoryItem newItem)
    {
        int index = GetFirst_EmptySlot();
        if (index == -1) return;
        inventoryItems[index] = newItem;
        UpdateSlotUI();
    }

    public void RemoveItem(int index)
    {
        inventoryItems[index] = null;
        UpdateSlotUI();
    }

    public void Move_Item(InventoryItem item, int index)
    {
        inventoryItems[index] = item;
        UpdateSlotUI();
    }

    public void SwapInventoryItems(int index1, int index2)
    {
        if (index1 < 0 || index1 >= inventoryItems.Length ||
            index2 < 0 || index2 >= inventoryItems.Length)
        {
            Debug.Log("index.");
            return;
        }

        InventoryItem temp = inventoryItems[index1];
        inventoryItems[index1] = inventoryItems[index2];
        inventoryItems[index2] = temp;

        UpdateSlotUI();
    }

    public bool CanAddItem()
    {
        if (GetFirst_EmptySlot() != -1)
        {
            return true;
        }

        return false;
    }

    public bool CanQuickItem()
    {
        if (GetFirst_EmptyQuickSlot() != -1)
        {
            return true;
        }

        return false;
    }

    private int GetFirst_EmptySlot()
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == null || inventoryItems[i].data == null)
                return i;
        }
        return -1;
    }

    private int GetFirst_EmptyQuickSlot()
    {
        for (int i = 0; i < quickSlotItems.Length; i++)
        {
            if (quickSlotItems[i] == null || quickSlotItems[i].data == null)
                return i;
        }
        return -1;
    }

    public bool GetCheck_QuiSlot_Item()
    {
        if (quickSlotItems[selectedQuickSlot]?.data == null)
        {
            return true;
        }
        return false;
    }

    public void Add_QuickSlot_Item(InventoryItem newItem)
    {
        int index = GetFirst_EmptyQuickSlot();
        if (index == -1) return;
        quickSlotItems[index] = newItem;
        UpdateSlotUI();
    }

    public void Move_QuickSlot_Item(InventoryItem item, int index)
    {
        quickSlotItems[index] = item;
        UpdateSlotUI();
    }

    public void Remove_QuickSlot_Item(int index)
    {
        quickSlotItems[index] = null;
        UpdateSlotUI();
    }

    public void SwapQuickItems(int index1, int index2)
    {
        if (index1 < 0 || index1 >= quickSlotItems.Length ||
            index2 < 0 || index2 >= quickSlotItems.Length)
        {
            Debug.Log("index.");
            return;
        }

        InventoryItem temp = quickSlotItems[index1];
        quickSlotItems[index1] = quickSlotItems[index2];
        quickSlotItems[index2] = temp;

        UpdateSlotUI();
    }

    public void SwapQuickAndInventoryItems(int quickSlotIndex, int inventoryIndex)
    {
        // 인덱스 범위 확인
        if (quickSlotIndex < 0 || quickSlotIndex >= quickSlotItems.Length ||
            inventoryIndex < 0 || inventoryIndex >= inventoryItems.Length)
        {
            Debug.Log("인덱스 범위를 벗어났습니다.");
            return;
        }

        // 아이템 교환
        InventoryItem temp = quickSlotItems[quickSlotIndex];
        quickSlotItems[quickSlotIndex] = inventoryItems[inventoryIndex];
        inventoryItems[inventoryIndex] = temp;

        // UI 업데이트
        UpdateSlotUI();
    }


    public List<InventoryItem> GetEquipmentList() => equipment;

    public InventoryItem[] GetInventoryList() => inventoryItems;

    public ItemData_Equipment GetEquipment(EquipmentType _type)
    {
        ItemData_Equipment equipedItem = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
        {
            if (item.Key.equipmentType == _type)
                equipedItem = item.Key;
        }

        return equipedItem;
    }

    public void SelectQuickSlot(int index)
    {
        selectedQuickSlot = index;
        UIManager.instance.QuickSlotUpdate();
        //Debug.Log("Quick Slot Selected: " + index);
    }
}