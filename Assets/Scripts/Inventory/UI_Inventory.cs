/*using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Scripts.Inventory
{
    public class UI_Inventory : MonoBehaviour
    {
        private UI_ItemSlot[] _bagSlots;
        private UI_QuickSlot[] _quickSlots;

        private Inventory_Base _inventory;

        private void Awake()
        {
            _bagSlots = GetComponentsInChildren<UI_ItemSlot>();
            _quickSlots = GetComponentsInChildren<UI_QuickSlot>();
            _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory_Base>();

            _inventory.UpdateUIObservable.Subscribe(_ =>
            {
                UpdateSlotsUI();
            });
        }

        private void UpdateSlotsUI()
        {
            List<Inventory_Item> quickList = _inventory.quickList;
            List<Inventory_Item> bagList = _inventory.quickList;
            
            for (int i = 0; i < _quickSlots.Length; i++)
            {
                if (i < quickList.Count)
                    _quickSlots[i].UpdateSlot(quickList[i]);
                else
                    _quickSlots[i].UpdateSlot(null);
            }
            
            for (int i = 0; i < _bagSlots.Length; i++)
            {
                if (i < bagList.Count)
                    _bagSlots[i].UpdateSlot(bagList[i]);
                else
                    _bagSlots[i].UpdateSlot(null);
            }
        }
    }
}*/