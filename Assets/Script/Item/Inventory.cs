using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<InventoryItem> equipment;
    public Dictionary<ItemData_Equipment, InventoryItem> equipmentDictionary;

    [SerializeField]
    public List<InventoryItem> inventoryItems;

    [Header("Inventory UI")]
    [SerializeField] private Transform inventorySlotParent;
    [SerializeField] private Transform equipmentSlotParent;

    private UI_ItemSlot[] inventoryItemSlot;
    private UI_EquipmentSlot[] equipmentSlot;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(instance);
    }

    private void Start()
    {
        inventoryItems = new List<InventoryItem>();

        equipment = new List<InventoryItem>();
        equipmentDictionary = new Dictionary<ItemData_Equipment, InventoryItem>();

        inventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();
    }

    public void EquipItem(ItemData _item)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
        InventoryItem newItem = new InventoryItem(newEquipment);

        ItemData_Equipment oldEquipment = null;

        foreach(KeyValuePair<ItemData_Equipment,InventoryItem> item in equipmentDictionary)
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
        RemoveItem(_item);

        UpdateSlotUI();
    }

    private void UnequipItem(ItemData_Equipment itemToRemove)
    {
        if(equipmentDictionary.TryGetValue(itemToRemove, out InventoryItem newItem))
        {
            equipment.Remove(newItem);
            equipmentDictionary.Remove(itemToRemove);
        }
    }

    private void UpdateSlotUI()
    {
        for(int i = 0; i < equipmentSlot.Length; i++)
        {
            foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictionary)
            {
                if (item.Key.equipmentType == equipmentSlot[i].slotType)
                    equipmentSlot[i].UpdateSlot(item.Value);
            }
        }

        for(int i =0; i < inventoryItemSlot.Length; i++)
        {
            inventoryItemSlot[i].CleanUpSlot();
        }

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            inventoryItemSlot[i].UpdateSlot(inventoryItems[i]);
        }
    }

    public void AddItem(ItemData _item)
    {
        if (CanAddItem())
        {
            InventoryItem newItem = new InventoryItem(_item);
            inventoryItems.Add(newItem);

            UpdateSlotUI();
        }
    }

    public void RemoveItem(ItemData _item)
    {
        // inventoryItems 리스트에서 _item과 같은 아이템을 찾아 제거
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data == _item)
            {
                inventoryItems.RemoveAt(i);
                break; // 하나만 삭제하고 종료
            }
        }

        UpdateSlotUI();
    }

    public bool CanAddItem()
    {
        if (inventoryItems.Count >= inventoryItemSlot.Length)
        {
            return false;
        }

        return true;
    }
}
