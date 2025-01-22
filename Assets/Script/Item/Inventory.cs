using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<InventoryItem> equipment;
    public Dictionary<ItemData, InventoryItem> equipmentDictionary;

    public List<InventoryItem> inventoryItems;

    [Header("Inventory UI")]
    [SerializeField] private Transform inventorySlotParent;

    private UI_ItemSlot[] itemSlot;

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
        equipmentDictionary = new Dictionary<ItemData, InventoryItem>();

        itemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
    }

    public void EquipItem(ItemData _item)
    {
        InventoryItem newItem = new InventoryItem(_item);

        equipment.Add(newItem);
        equipmentDictionary.Add(_item, newItem);
    }

    private void UpdateSlotUI()
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            itemSlot[i].UpdateSlot(inventoryItems[i]);
        }
    }

    public void AddItem(ItemData _item)
    {
        InventoryItem newItem = new InventoryItem(_item);
        inventoryItems.Add(newItem);
        //inventoryDictanory.Add(_item, newItem);

        UpdateSlotUI();
    }

    public void RemoveItem(ItemData _item)
    {
        InventoryItem itemToRemove = null;

        inventoryItems.Remove(itemToRemove);

        UpdateSlotUI();
    }
}
