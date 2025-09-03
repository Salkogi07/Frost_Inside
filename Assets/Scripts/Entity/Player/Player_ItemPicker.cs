using Unity.Netcode;
using UnityEngine;

public class Player_ItemPicker : MonoBehaviour
{
    [Tooltip("아이템을 주울 수 있는 최대 거리")]
    public float pickupRange = 3f;

    [Tooltip("아이템을 줍고 난 후의 딜레이 (초)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    void Update()
    {
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
                ItemObject itemPickupComponent = nearestItemObject.GetComponent<ItemObject>();
                if (itemPickupComponent != null)
                {
                    // 아이템 줍기 시도
                    TryPickupItem(nearestItemObject, itemPickupComponent);
                }
            }
        }
    }

    /// <summary>
    /// 아이템 줍기를 시도하고, 성공하면 아이템을 파괴합니다.
    /// </summary>
    /// <param name="itemObject">주울 아이템의 게임 오브젝트</param>
    /// <param name="itemPickupComponent">아이템의 데이터가 담긴 ItemObject 컴포넌트</param>
    private void TryPickupItem(GameObject itemObject, ItemObject itemPickupComponent)
    {
        InventoryManager inventory = InventoryManager.Instance;
        Inventory_Item newItem = itemPickupComponent.GetItemData();
        
        // 현재 선택된 퀵슬롯이 비어있으면 그곳에 아이템을 바로 추가
        if (inventory.quickSlotItems[inventory.selectedQuickSlot].IsEmpty())
        {
            // SetItem으로 아이템을 직접 할당
            inventory.SetItem(SlotType.QuickSlot, inventory.selectedQuickSlot, newItem);
            // UI를 수동으로 갱신
            InventoryUI.Instance.UpdateAllSlots();
            
            itemObject.GetComponent<NetworkObject>().Despawn();
            nextPickupTime = Time.time + pickupCooldown; // 줍기 성공 시 쿨다운 적용
            return;
        }
        
        if (inventory.isPocket && inventory.CanAddItem())
        {
            // AddItem은 내부적으로 빈 슬롯을 찾아 아이템을 추가하고 UI를 갱신
            inventory.AddItem(newItem);
            
            itemObject.GetComponent<NetworkObject>().Despawn();
            nextPickupTime = Time.time + pickupCooldown; // 줍기 성공 시 쿨다운 적용
            return;
        }
        
        Debug.Log("인벤토리가 가득 찼습니다!");
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