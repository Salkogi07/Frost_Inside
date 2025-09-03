using UnityEngine;
using Unity.Netcode;

// NetworkBehaviour를 상속받아 ServerRpc와 ClientRpc를 사용할 수 있도록 변경합니다.
public class Player_ItemPicker : NetworkBehaviour
{
    [Tooltip("아이템을 주울 수 있는 최대 거리")]
    public float pickupRange = 3f;

    [Tooltip("아이템을 줍고 난 후의 딜레이 (초)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    // IsOwner는 이 스크립트가 내 플레이어 캐릭터에 있는지 확인합니다.
    // 이렇게 하면 다른 플레이어 캐릭터를 내 입력으로 조종하는 것을 방지할 수 있습니다.
    void Update()
    {
        if (!IsOwner) return;

        if (InventoryManager.Instance.isInvenOpen)
            return;

        if (Time.time >= nextPickupTime && Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Pick Up Item")))
        {
            if (InventoryManager.Instance.CanAddItem() || InventoryManager.Instance.quickSlotItems[InventoryManager.Instance.selectedQuickSlot].IsEmpty())
            {
                GameObject nearestItemObject = FindNearestItem();
                if (nearestItemObject != null)
                {
                    // 서버에 아이템을 주워달라고 요청합니다.
                    PickupItemServerRpc(nearestItemObject.GetComponent<NetworkObject>());
                }
            }
            else
            {
                Debug.Log("인벤토리가 가득 찼습니다!");
            }
        }
    }
    
    /// <summary>
    /// (서버에서 실행됨) 클라이언트가 아이템 줍기를 요청하면 호출됩니다.
    /// </summary>
    [ServerRpc]
    private void PickupItemServerRpc(NetworkObjectReference itemObjectRef)
    {
        // NetworkObjectReference로부터 실제 NetworkObject를 가져옵니다.
        if (itemObjectRef.TryGet(out NetworkObject itemNetworkObject))
        {
            // 거리 체크 등 추가적인 유효성 검사를 서버에서 수행할 수 있습니다.
            // 예: Vector3.Distance(transform.position, itemNetworkObject.transform.position) > pickupRange
            
            ItemObject itemPickupComponent = itemNetworkObject.GetComponent<ItemObject>();
            if (itemPickupComponent == null) return;

            Inventory_Item itemToPickup = itemPickupComponent.GetItemData();

            // 아이템을 성공적으로 주웠다는 신호를 클라이언트에게 보냅니다.
            PickupItemClientRpc(itemToPickup);

            // 아이템 오브젝트를 Despawn하고 파괴합니다. 이것이 올바른 방법입니다.
            itemNetworkObject.Despawn(true);
        }
    }

    /// <summary>
    /// (클라이언트에서 실행됨) 서버가 아이템 줍기를 승인했을 때 호출됩니다.
    /// </summary>
    [ClientRpc]
    private void PickupItemClientRpc(Inventory_Item pickedUpItem)
    {
        // 로컬 인벤토리에 아이템을 추가합니다.
        InventoryManager inventory = InventoryManager.Instance;
        
        // 퀵슬롯이 비어있으면 퀵슬롯에 먼저 추가
        if (inventory.quickSlotItems[inventory.selectedQuickSlot].IsEmpty())
        {
            inventory.SetItem(SlotType.QuickSlot, inventory.selectedQuickSlot, pickedUpItem);
        }
        // 퀵슬롯이 차있으면 일반 인벤토리에 추가
        else if (inventory.CanAddItem())
        {
            inventory.AddItem(pickedUpItem);
        }

        InventoryUI.Instance.UpdateAllSlots(); // UI 갱신
        nextPickupTime = Time.time + pickupCooldown; // 쿨다운 적용
    }

    /// <summary>
    /// 설정된 범위 내에서 가장 가까운 "Item" 태그를 가진 게임 오브젝트를 찾습니다.
    /// </summary>
    /// <returns>가장 가까운 아이템 오브젝트 또는 null</returns>
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