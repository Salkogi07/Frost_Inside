using System;
using UnityEngine;
using Unity.Netcode;

public class Player_ItemPicker : NetworkBehaviour
{
    private Player_Condition _player_condition;
    
    public float pickupRange = 3f;
    public float pickupCooldown = 0.5f;
    private float nextPickupTime = 0f;

    private void Awake()
    {
        _player_condition = GetComponent<Player_Condition>();
    }

    void Update()
    {
        if (!IsOwner) return;
        
        if(_player_condition.CheckIsDead())
            return;

        if (InventoryManager.instance.isInvenOpen || Time.time < nextPickupTime)
            return;
        
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Pick Up Item")))
        {
            bool canQuickSlot = InventoryManager.instance.quickSlotItems[InventoryManager.instance.selectedQuickSlot].IsEmpty();
            bool canPocket = InventoryManager.instance.isPocket && InventoryManager.instance.CanAddItem();

            if (canQuickSlot || canPocket)
            {
                GameObject nearestItemObject = FindNearestItem();
                if (nearestItemObject != null)
                {
                    PickupItemServerRpc(nearestItemObject.GetComponent<NetworkObject>());
                }
            }
            else
            {
                Debug.Log("인벤토리가 가득 찼습니다!");
            }
        }
    }

    [ServerRpc]
    private void PickupItemServerRpc(NetworkObjectReference itemObjectRef, ServerRpcParams rpcParams = default)
    {
        ulong requesterClientId = rpcParams.Receive.SenderClientId;
        
        if (!itemObjectRef.TryGet(out NetworkObject itemNetworkObject))
        {
            return; 
        }

        ItemObject itemPickupComponent = itemNetworkObject.GetComponent<ItemObject>();
        if (itemPickupComponent == null || !itemNetworkObject.gameObject.activeInHierarchy) return;

        Inventory_Item itemToPickup = itemPickupComponent.GetItemData();
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { requesterClientId }
            }
        };
        ConfirmPickupClientRpc(itemToPickup, clientRpcParams);

        // === 오브젝트 풀링 반환 부분 ===
        // 1. 재사용을 위해 아이템 상태를 리셋합니다.
        itemPickupComponent.ResetForPool();
        // 2. 오브젝트를 파괴하는 대신 풀에 반환합니다.
        NetworkItemPool.Instance.ReturnObjectToPool(itemNetworkObject);
    }

    [ClientRpc]
    private void ConfirmPickupClientRpc(Inventory_Item pickedUpItem, ClientRpcParams rpcParams = default)
    {
        InventoryManager inventory = InventoryManager.instance;
        
        if (inventory.quickSlotItems[inventory.selectedQuickSlot].IsEmpty())
        {
            inventory.SetItem(SlotType.QuickSlot, inventory.selectedQuickSlot, pickedUpItem);
        }
        else if (inventory.isPocket && inventory.CanAddItem())
        {
            inventory.AddItem(pickedUpItem);
        }

        InventoryUI.Instance.UpdateAllSlots();
        nextPickupTime = Time.time + pickupCooldown;
    }
    
    GameObject FindNearestItem()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject item in items)
        {
            // 비활성화된 (풀링된) 아이템은 찾지 않도록 조건 추가
            if (!item.activeInHierarchy) continue;

            float distance = Vector3.Distance(item.transform.position, currentPos);
            if (distance <= pickupRange && distance < minDistance)
            {
                nearest = item;
                minDistance = distance;
            }
        }
        return nearest;
    }
}