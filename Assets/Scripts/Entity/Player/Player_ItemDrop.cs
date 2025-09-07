using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class Player_ItemDrop : NetworkBehaviour
{
    [SerializeField] private Vector2 dropVec;
    
    public void DropItem(SlotType slotType, int slotIndex)
    {
        if(GameManager.instance.itemSpawner == null)
            return;
        
        Inventory_Item itemToDrop = InventoryManager.Instance.GetItem(slotType, slotIndex);
        if (itemToDrop.IsEmpty()) return;

        GameObject dropObject = gameObject.GetComponent<Player>().playerObject;
        Vector2 velocity = new Vector2(dropVec.x * dropObject.transform.localScale.x * -1, dropVec.y);

        // 서버에 아이템 생성을 요청
        SpawnItemServerRpc(itemToDrop, dropObject.transform.position, velocity);

        // 해당 슬롯 비우기
        InventoryManager.Instance.SetItem(slotType, slotIndex, Inventory_Item.Empty);
        InventoryUI.Instance.UpdateAllSlots();
    }

    public void NotPocket_ItemDrop()
    {
        if(GameManager.instance.itemSpawner == null)
            return;
        
        // 인벤토리 아이템 드롭
        for (int i = 0; i < InventoryManager.Instance.inventoryItems.Length; i++)
        {
            DropItemWithRandomVelocity(InventoryManager.Instance.inventoryItems[i]);
            InventoryManager.Instance.inventoryItems[i] = Inventory_Item.Empty;
        }
    }
    
    public void DropAllItems()
    {
        if (!IsOwner) return;
        
        if(GameManager.instance.itemSpawner == null)
            return;

        // 인벤토리 아이템 드롭
        for (int i = 0; i < InventoryManager.Instance.inventoryItems.Length; i++)
        {
            DropItemWithRandomVelocity(InventoryManager.Instance.inventoryItems[i]);
            InventoryManager.Instance.inventoryItems[i] = Inventory_Item.Empty;
        }

        // 퀵슬롯 아이템 드롭
        for (int i = 0; i < InventoryManager.Instance.quickSlotItems.Length; i++)
        {
            DropItemWithRandomVelocity(InventoryManager.Instance.quickSlotItems[i]);
            InventoryManager.Instance.quickSlotItems[i] = Inventory_Item.Empty;
        }

        // 장비 아이템 드롭
        for (int i = 0; i < InventoryManager.Instance.equipmentItems.Length; i++)
        {
            DropItemWithRandomVelocity(InventoryManager.Instance.equipmentItems[i]);
            InventoryManager.Instance.equipmentItems[i] = Inventory_Item.Empty;
        }
        
        InventoryUI.Instance.UpdateAllSlots();
    }
    
    private void DropItemWithRandomVelocity(Inventory_Item item)
    {
        if(GameManager.instance.itemSpawner == null)
            return;
        
        if (item.IsEmpty()) return;
        
        Vector2 dropPosition = transform.position;
        Vector2 velocity = new Vector2(Random.Range(-5,5), Random.Range(15,20));
        SpawnItemServerRpc(item, dropPosition, velocity);
    }
    
    [ServerRpc]
    private void SpawnItemServerRpc(Inventory_Item itemToSpawn, Vector3 position, Vector2 velocity)
    {
        // 서버에서만 실행되는 코드
        GameManager.instance.itemSpawner.SpawnPlayerDroppedItem(itemToSpawn, position, velocity);
    }
}
