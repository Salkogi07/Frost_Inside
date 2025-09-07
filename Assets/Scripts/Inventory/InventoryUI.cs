using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    
    [Header("Inventory & Poket Panels")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject poketPanel;
    public UI_ItemToolTip itemToolTip;
    
    [Header("Slot Parents")]
    [SerializeField] private Transform poketSlotParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform quickSlotParent;
    [SerializeField] private Transform quickSlotViewerParent;
    
    private UI_ItemSlot[] inventorySlots;
    private UI_EquipmentSlot[] equipmentSlots;
    private UI_QuickSlot[] quickSlots;
    private UI_QuickSlotViewer[] quickSlotsViewer;

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

        SettingUI();
    }

    private void SettingUI()
    {
        inventorySlots = poketSlotParent.GetComponentsInChildren<UI_ItemSlot>();
        equipmentSlots = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();
        quickSlots = quickSlotParent.GetComponentsInChildren<UI_QuickSlot>();
        quickSlotsViewer = quickSlotViewerParent.GetComponentsInChildren<UI_QuickSlotViewer>();

        // 각 슬롯에 인덱스 할당
        for (int i = 0; i < inventorySlots.Length; i++) inventorySlots[i].slotIndex = i;
        for (int i = 0; i < equipmentSlots.Length; i++) equipmentSlots[i].slotIndex = i;
        for (int i = 0; i < quickSlots.Length; i++) quickSlots[i].slotIndex = i;
        for (int i = 0; i < quickSlotsViewer.Length; i++) quickSlotsViewer[i].slotIndex = i;

        // InventoryManager에 슬롯 크기 정보 전달
        InventoryManager.Instance.InitializeSlots(inventorySlots.Length, quickSlots.Length);
    }
    
    public void UpdateAllSlots()
    {
        UpdateInventorySlots();
        UpdateQuickSlots();
        UpdateEquipmentSlots();
        UpdateQuickSlotsViewer();
        InventoryManager.Instance.UpdatePlayerWeight();
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
        }
    }
    
    private void UpdateQuickSlotsViewer()
    {
        for (int i = 0; i < quickSlotsViewer.Length; i++)
        {
            quickSlotsViewer[i].UpdateSlot(InventoryManager.Instance.quickSlotItems[i]);
            
            // 선택된 퀵슬롯 UI 처리
            quickSlotsViewer[i].SetHighlight(i == InventoryManager.Instance.selectedQuickSlot);
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
    
    public void UpdatePoketPanel()
    {
        poketPanel.gameObject.SetActive(InventoryManager.Instance.isPocket);
    }
}