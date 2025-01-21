using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<InventoryItem> inventoryItems;
    //public Dictionary<ItemData, InventoryItem> inventoryDictanory;

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
        //inventoryDictanory = new Dictionary<ItemData, InventoryItem>();

        itemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
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
        /*if(inventoryDictanory.TryGetValue(_item, out InventoryItem value))
        {
            
            inventoryDictanory.Remove(_item);
        }*/

        inventoryItems.Remove(itemToRemove);

        UpdateSlotUI();
    }
}
