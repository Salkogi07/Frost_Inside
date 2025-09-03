// Player_ItemPicker.cs

using UnityEngine;
using Unity.Netcode; // Netcode 네임스페이스 추가

// NetworkBehaviour를 상속받아야 ServerRpc를 사용할 수 있습니다.
public class Player_ItemPicker : NetworkBehaviour 
{
    // Player_Stats stats; // 필요시 주석 해제하여 사용

    [Tooltip("아이템을 주울 수 있는 최대 거리")]
    public float pickupRange = 3f;

    [Tooltip("아이템을 줍고 난 후의 딜레이 (초)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    private void Awake()
    {
        // stats = GetComponent<Player_Stats>(); // 필요시 주석 해제하여 사용
    }

    void Update()
    {
        if (!IsOwner) return;

        // if(stats.isDead) return;

        // 인벤토리가 열려있으면 아이템 줍기 비활성화
        if (InventoryManager.Instance.isInvenOpen)
            return;
        
        // 쿨다운이 지났고, 줍기 키를 눌렀을 때
        if (Time.time >= nextPickupTime && Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Pick Up Item")))
        {
            GameObject nearestItemObject = FindNearestItem();
            if (nearestItemObject != null)
            {
                // 아이템 줍기를 서버에 요청합니다.
                RequestPickupItemServerRpc(nearestItemObject.GetComponent<NetworkObject>());
            }
        }
    }

    /// <summary>
    /// [ServerRpc] 서버에게 아이템 줍기를 요청합니다.
    /// </summary>
    [ServerRpc]
    private void RequestPickupItemServerRpc(NetworkObjectReference itemNetworkObjectRef)
    {
        // NetworkObjectReference로부터 실제 NetworkObject를 가져옵니다.
        if (itemNetworkObjectRef.TryGet(out NetworkObject itemNetworkObject))
        {
            // 서버에서 실제 거리와 같은 유효성 검사를 다시 수행하는 것이 안전합니다.
            if (Vector3.Distance(transform.position, itemNetworkObject.transform.position) > pickupRange)
            {
                return; // 거리가 너무 멀면 무시
            }

            ItemObject itemPickupComponent = itemNetworkObject.GetComponent<ItemObject>();
            if (itemPickupComponent != null)
            {
                // 서버에서 아이템 줍기를 시도합니다.
                TryPickupItemOnServer(itemNetworkObject.gameObject, itemPickupComponent);
            }
        }
    }

    /// <summary>
    /// (서버 전용 함수) 아이템 줍기를 시도하고, 성공하면 아이템을 파괴합니다.
    /// </summary>
    private void TryPickupItemOnServer(GameObject itemObject, ItemObject itemPickupComponent)
    {
        InventoryManager inventory = InventoryManager.Instance;
        Inventory_Item newItem = itemPickupComponent.GetItemData();
        
        // 퀵슬롯이 비어있는 경우
        if (inventory.quickSlotItems[inventory.selectedQuickSlot].IsEmpty())
        {
            inventory.SetItem(SlotType.QuickSlot, inventory.selectedQuickSlot, newItem);
            InventoryUI.Instance.UpdateAllSlots();
            
            // 아이템 오브젝트를 네트워크에서 Despawn하고 파괴합니다.
            itemObject.GetComponent<NetworkObject>().Despawn();
            
            return;
        }
        
        // 포켓(기본 인벤토리)에 공간이 있는 경우
        if (inventory.isPocket && inventory.CanAddItem())
        {
            inventory.AddItem(newItem);
            
            itemObject.GetComponent<NetworkObject>().Despawn();
            return;
        }
        
        Debug.Log("인벤토리가 가득 찼습니다! (서버 로그)");
    }
    
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