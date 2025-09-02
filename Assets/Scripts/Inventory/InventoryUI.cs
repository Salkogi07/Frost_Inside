using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    
    [Header("Inventory")]
    [SerializeField] private GameObject inventoryPanel;
    
    [Header("Slot Parents")]
    [SerializeField] private Transform poketSlotParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform quickSlotParent;

    private UI_ItemSlot[] inventorySlots;
    private UI_EquipmentSlot[] equipmentSlots;
    private UI_QuickSlot[] quickSlots;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        inventorySlots = poketSlotParent.GetComponentsInChildren<UI_ItemSlot>();
        equipmentSlots = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();
        quickSlots = quickSlotParent.GetComponentsInChildren<UI_QuickSlot>();

        // 각 슬롯에 인덱스 할당
        for (int i = 0; i < inventorySlots.Length; i++) inventorySlots[i].slotIndex = i;
        for (int i = 0; i < equipmentSlots.Length; i++) equipmentSlots[i].slotIndex = i;
        for (int i = 0; i < quickSlots.Length; i++) quickSlots[i].slotIndex = i;

        // InventoryManager에 슬롯 크기 정보 전달
        InventoryManager.Instance.InitializeSlots(inventorySlots.Length, quickSlots.Length);
    }
    
    public void UpdateAllSlots()
    {
        UpdateInventorySlots();
        UpdateQuickSlots();
        UpdateEquipmentSlots();
    }

    private void UpdateInventorySlots()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].UpdateSlot(InventoryManager.Instance.inventoryItems[i]);
        }
    }

    private void UpdateQuickSlots()
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].UpdateSlot(InventoryManager.Instance.quickSlotItems[i]);
            // 선택된 퀵슬롯 UI 처리
            quickSlots[i].SetHighlight(i == InventoryManager.Instance.selectedQuickSlot);
        }
    }

    private void UpdateEquipmentSlots()
    {
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            equipmentSlots[i].UpdateSlot(InventoryManager.Instance.equipmentItems[i]);
        }
    }

    public void UpdateInventoryPanel()
    {
        inventoryPanel.gameObject.SetActive(InventoryManager.Instance.isInvenOpen);
    }
}