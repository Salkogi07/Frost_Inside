using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<ItemData> startingItems;

    public InventoryItem[] quickSlotItems;
    private int selectedQuickSlot = 0;

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
            instance = this;
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
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectQuickSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectQuickSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectQuickSlot(2);

        Image selcetquickSlot = quickSlot[selectedQuickSlot].gameObject.GetComponent<Image>();
        Image quickSlot1 = quickSlot[0].gameObject.GetComponent<Image>();
        Image quickSlot2 = quickSlot[1].gameObject.GetComponent<Image>();
        Image quickSlot3 = quickSlot[2].gameObject.GetComponent<Image>();

        quickSlot1.color = Color.clear;
        quickSlot2.color = Color.clear;
        quickSlot3.color = Color.clear;
        selcetquickSlot.color = Color.red;

        if (Input.GetAxis("Mouse ScrollWheel") > 0) SelectQuickSlot((selectedQuickSlot + 1) % 3);
        if (Input.GetAxis("Mouse ScrollWheel") < 0) SelectQuickSlot((selectedQuickSlot + 2) % 3);
    }

    private void AddStartingItems()
    {
        for (int i = 0; i < startingItems.Count; i++)
        {
            AddItem(startingItems[i]);
        }
    }

    public void EquipItem(ItemData _item, int index)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
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
            AddItem(oldEquipment);
        }

        equipment.Add(newItem);
        equipmentDictionary.Add(newEquipment, newItem);
        newEquipment.AddModifiers();

        RemoveItem(_item, index);

        UpdateSlotUI();
    }

    public void UnequipItem(ItemData_Equipment itemToRemove)
    {
        if (equipmentDictionary.TryGetValue(itemToRemove, out InventoryItem newItem))
        {
            equipment.Remove(newItem);
            equipmentDictionary.Remove(itemToRemove);
            itemToRemove.RemoveModifiers();
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

    public void AddItem(ItemData _item)
    {
        int index = GetFirst_EmptySlot();
        if (index == -1)
        {
            Debug.Log("Full Inventory!");
            return;
        }
        InventoryItem newItem = new InventoryItem(_item);
        inventoryItems[index] = newItem;
        UpdateSlotUI();
    }


    public void RemoveItem(ItemData _item, int index)
    {
        inventoryItems[index] = null;

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

    private int GetFirst_EmptySlot()
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == null || inventoryItems[i].data == null)
                return i;
        }
        return -1;
    }

    public void Move_QuickSlot_Item(ItemData _item , int index)
    {
        InventoryItem newItem = new InventoryItem(_item);
        quickSlotItems[index] = newItem;
        UpdateSlotUI();
    }


    public void Remove_QuickSlot_Item(ItemData _item, int index)
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
        Debug.Log("Quick Slot Selected: " + index);
    }
}