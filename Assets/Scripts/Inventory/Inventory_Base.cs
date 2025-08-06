using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Scripts.Inventory
{
    public class Inventory_Base : MonoBehaviour
    {
        public bool isPocket = false;
        public bool isInvenOpen = false;
        
        public int selectedQuickSlot = 0;
        
        public int maxBagSize = 4;
        public int maxQuickSize = 4;
        public List<Inventory_Item> bagList = new List<Inventory_Item>();
        public List<Inventory_Item> quickList = new List<Inventory_Item>();
        
        private Subject<Unit> UpdateUI = new  Subject<Unit>();
        public Observable<Unit> UpdateUIObservable => UpdateUI.AsObservable();
        
        public bool CanAddBag => bagList.Count < maxQuickSize;
        public bool CanAddQuick => bagList.Count < maxQuickSize;
        
        private Player_ItemDrop _playerItemDrop;

        private void Awake()
        {
            _playerItemDrop = GetComponent<Player_ItemDrop>();
        }

        private void Update()
        {
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
                if (scroll < 0) SelectQuickSlot((selectedQuickSlot + 1) % maxQuickSize);
                if (scroll > 0) SelectQuickSlot((selectedQuickSlot + 2) % maxQuickSize);

                if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Throw Item")))
                {
                    if(quickList[selectedQuickSlot]?.data != null)
                    {
                        _playerItemDrop.QuickSlot_Throw(quickList[selectedQuickSlot].data, selectedQuickSlot);
                        return;
                    }
                }
            }
        }

        public void AddBag(Inventory_Item item)
        {
            bagList.Add(item);
            UpdateUI.OnNext(Unit.Default);
        }
        
        public void AddQuick(Inventory_Item item)
        {
            quickList.Add(item);
            UpdateUI.OnNext(Unit.Default);
        }
        
        private void NotPocket_ItemDrop()
        {
            for (int i = 0; i < bagList.Count; i++)
            {
                if (bagList[i]?.data != null)
                {
                    _playerItemDrop.Pocket_Inventory_Drop(bagList[i].data, i);
                    bagList[i] = null;
                }
            }
        }
        
        public void SelectQuickSlot(int index)
        {
            selectedQuickSlot = index;
            UIManager.instance.QuickSlotUpdate();
            //Debug.Log("Quick Slot Selected: " + index);
        }
    }
}