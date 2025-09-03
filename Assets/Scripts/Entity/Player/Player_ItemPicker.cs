// Player_ItemPicker.cs

using UnityEngine;
using Unity.Netcode;

public class Player_ItemPicker : NetworkBehaviour 
{
    [Tooltip("아이템을 주울 수 있는 최대 거리")]
    public float pickupRange = 3f;

    void Update()
    {
        // 이 스크립트는 로컬 플레이어만 조작할 수 있어야 합니다.
        if (!IsOwner) return;

        // 인벤토리가 열려있으면 줍기 불가
        if (InventoryManager.Instance.isInvenOpen) return;
        
        // 줍기 키를 눌렀을 때
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Pick Up Item")))
        {
            // [클라이언트] 가장 가까운 아이템을 찾습니다.
            GameObject nearestItemObject = FindNearestItem();
            if (nearestItemObject != null)
            {
                // [클라이언트] 자신의 인벤토리에 공간이 있는지 먼저 확인합니다.
                if (CanPickupItem())
                {
                    // [클라이언트] 서버에게 아이템 줍기를 요청합니다.
                    RequestPickupItemServerRpc(nearestItemObject.GetComponent<NetworkObject>());
                }
                else
                {
                    Debug.Log("인벤토리가 가득 찼습니다!"); // 클라이언트 측에서 즉시 피드백
                }
            }
        }
    }

    /// <summary>
    /// [ServerRpc] 서버에게 아이템 줍기를 요청합니다. 클라이언트가 호출하면 서버에서 실행됩니다.
    /// </summary>
    [ServerRpc]
    private void RequestPickupItemServerRpc(NetworkObjectReference itemNetworkObjectRef, ServerRpcParams rpcParams = default)
    {
        // NetworkObjectReference로부터 실제 NetworkObject를 가져옵니다.
        if (!itemNetworkObjectRef.TryGet(out NetworkObject itemNetworkObject)) return;
        
        // [서버] 거리 등 기본적인 유효성 검사를 수행합니다.
        if (Vector3.Distance(transform.position, itemNetworkObject.transform.position) > pickupRange)
        {
            return; // 너무 멀면 무시
        }

        ItemObject itemPickupComponent = itemNetworkObject.GetComponent<ItemObject>();
        if (itemPickupComponent == null) return;

        // [서버] 요청한 클라이언트에게만 "아이템을 획득했다"고 응답(ClientRpc)을 보냅니다.
        Inventory_Item itemData = itemPickupComponent.GetItemData();
        ulong requestingClientId = rpcParams.Receive.SenderClientId;
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { requestingClientId }
            }
        };
        AcknowledgePickupClientRpc(itemData, clientRpcParams);

        // [서버] 모든 클라이언트에서 아이템을 제거하도록 Despawn 합니다.
        itemNetworkObject.Despawn();
    }

    /// <summary>
    /// [ClientRpc] 서버가 아이템 획득을 승인했음을 특정 클라이언트에게 알립니다.
    /// </summary>
    [ClientRpc]
    private void AcknowledgePickupClientRpc(Inventory_Item itemData, ClientRpcParams clientRpcParams = default)
    {
        // [클라이언트] 서버로부터 받은 아이템을 로컬 인벤토리에 추가합니다.
        InventoryManager inventory = InventoryManager.Instance;
        
        // 퀵슬롯이 비어있는 경우
        if (inventory.quickSlotItems[inventory.selectedQuickSlot].IsEmpty())
        {
            inventory.SetItem(SlotType.QuickSlot, inventory.selectedQuickSlot, itemData);
        }
        // 포켓(기본 인벤토리)에 공간이 있는 경우
        else if (inventory.isPocket && inventory.CanAddItem())
        {
            inventory.AddItem(itemData);
        }
        
        InventoryUI.Instance.UpdateAllSlots();
    }
    
    /// <summary>
    /// [클라이언트 전용] 자신의 로컬 인벤토리에 아이템을 추가할 공간이 있는지 확인합니다.
    /// </summary>
    private bool CanPickupItem()
    {
        InventoryManager inventory = InventoryManager.Instance;
        return inventory.quickSlotItems[inventory.selectedQuickSlot].IsEmpty() || (inventory.isPocket && inventory.CanAddItem());
    }
    
    private GameObject FindNearestItem()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject item in items)
        {
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