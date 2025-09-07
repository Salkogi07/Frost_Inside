using UnityEngine;
using Unity.Netcode;

public class Player_ItemPicker : NetworkBehaviour
{
    [Tooltip("아이템을 주울 수 있는 최대 거리")]
    public float pickupRange = 3f;

    [Tooltip("아이템을 줍고 난 후의 딜레이 (초)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    /// <summary>
    /// IsOwner는 이 스크립트가 내 플레이어 캐릭터에 있는지 확인합니다.
    /// 이렇게 하면 다른 플레이어의 입력을 내 캐릭터가 받는 것을 방지할 수 있습니다.
    /// </summary>
    void Update()
    {
        if (!IsOwner) return;

        // 인벤토리가 열려있거나 쿨다운 중이면 줍기 불가
        if (InventoryManager.instance.isInvenOpen || Time.time < nextPickupTime)
            return;
        
        // (클라이언트) 줍기 키를 눌렀을 때
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Pick Up Item")))
        {
            // (클라이언트) 인벤토리에 공간이 있는지 먼저 확인
            bool canQuickSlot = InventoryManager.instance.quickSlotItems[InventoryManager.instance.selectedQuickSlot].IsEmpty();
            bool canPocket = InventoryManager.instance.isPocket && InventoryManager.instance.CanAddItem();

            if (canQuickSlot || canPocket)
            {
                GameObject nearestItemObject = FindNearestItem();
                if (nearestItemObject != null)
                {
                    // (클라이언트 -> 서버) 서버에 아이템을 주워달라고 요청
                    PickupItemServerRpc(nearestItemObject.GetComponent<NetworkObject>());
                }
            }
            else
            {
                // 인벤토리가 가득 찼음을 로컬에서 즉시 피드백
                Debug.Log("인벤토리가 가득 찼습니다!");
            }
        }
    }

    /// <summary>
    /// [ServerRpc] 클라이언트가 아이템 줍기를 요청하면 서버에서 실행됩니다.
    /// ServerRpcParams를 통해 어떤 클라이언트가 요청했는지 알 수 있습니다.
    /// </summary>
    [ServerRpc]
    private void PickupItemServerRpc(NetworkObjectReference itemObjectRef, ServerRpcParams rpcParams = default)
    {
        // 요청을 보낸 클라이언트의 ID를 가져옵니다.
        ulong requesterClientId = rpcParams.Receive.SenderClientId;
        
        // NetworkObjectReference로부터 실제 NetworkObject를 가져옵니다.
        if (!itemObjectRef.TryGet(out NetworkObject itemNetworkObject))
        {
            // 유효하지 않은 아이템 오브젝트 참조인 경우, 아무것도 하지 않음
            return; 
        }

        // 서버에서도 거리가 유효한지 한번 더 체크하여 핵 등을 방지할 수 있습니다.
        // if (Vector3.Distance(transform.position, itemNetworkObject.transform.position) > pickupRange * 1.2f) return;

        ItemObject itemPickupComponent = itemNetworkObject.GetComponent<ItemObject>();
        if (itemPickupComponent == null) return;

        // (서버) 아이템 데이터를 가져옴
        Inventory_Item itemToPickup = itemPickupComponent.GetItemData();
        
        // (서버 -> 클라이언트) 요청을 보낸 "특정 클라이언트"에게 아이템을 추가하라고 명령
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { requesterClientId }
            }
        };
        ConfirmPickupClientRpc(itemToPickup, clientRpcParams);

        // (서버) 모든 클라이언트에게서 아이템을 제거 (Despawn)
        itemNetworkObject.Despawn(true);
    }

    /// <summary>
    /// [ClientRpc] 서버가 아이템 줍기를 승인했을 때 "지정된" 클라이언트에서만 실행됩니다.
    /// </summary>
    [ClientRpc]
    private void ConfirmPickupClientRpc(Inventory_Item pickedUpItem, ClientRpcParams rpcParams = default)
    {
        // (클라이언트) 인벤토리에 아이템을 최종적으로 추가
        InventoryManager inventory = InventoryManager.instance;
        
        // 퀵슬롯이 비어있으면 퀵슬롯에 먼저 추가
        if (inventory.quickSlotItems[inventory.selectedQuickSlot].IsEmpty())
        {
            inventory.SetItem(SlotType.QuickSlot, inventory.selectedQuickSlot, pickedUpItem);
        }
        // 퀵슬롯이 차있고 포켓이 열려있으면 일반 인벤토리에 추가
        else if (inventory.isPocket && inventory.CanAddItem())
        {
            inventory.AddItem(pickedUpItem);
        }

        InventoryUI.Instance.UpdateAllSlots(); // UI 갱신
        nextPickupTime = Time.time + pickupCooldown; // 쿨다운 적용
    }
    
    /// <summary>
    /// 설정된 범위 내에서 가장 가까운 "Item" 태그를 가진 게임 오브젝트를 찾습니다.
    /// </summary>
    GameObject FindNearestItem()
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