using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<ItemData> startingItems;

    public List<InventoryItem> equipment;
    public Dictionary<ItemData_Equipment, InventoryItem> equipmentDictionary;

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
        AddStartingItems();
    }

    private void AddStartingItems()
    {
        for (int i = 0; i < startingItems.Count; i++)
        {
            AddItem(startingItems[i]);
        }
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
        newEquipment.AddModifiers();

        RemoveItem(_item);

        UpdateSlotUI();
    }

    public void UnequipItem(ItemData_Equipment itemToRemove)
    {
        if(equipmentDictionary.TryGetValue(itemToRemove, out InventoryItem newItem))
        {
            equipment.Remove(newItem);
            equipmentDictionary.Remove(itemToRemove);
            itemToRemove.RemoveModifiers();
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
        // inventoryItems ����Ʈ���� _item�� ���� �������� ã�� ����
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].data == _item)
            {
                inventoryItems.RemoveAt(i);
                break; // �ϳ��� �����ϰ� ����
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

    public List<InventoryItem> GetEquipmentList() => equipment;

    public List<InventoryItem> GetInventoryList() => inventoryItems;
}
