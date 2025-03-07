using Unity.VisualScripting;
using UnityEngine;

public class Player_ItemPicker : MonoBehaviour
{
    Player_Move player_move;
    Player_Stats stats;

    [Tooltip("아이템을 줍기 위한 최대 거리")]
    public float pickupRange = 3f;

    [Tooltip("아이템 줍기 후 쿨다운 시간 (초)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    private void Awake()
    {
        player_move = GetComponent<Player_Move>();
        stats = GetComponent<Player_Stats>();
    }

    void Update()
    {
        if(stats.isDead)
            return;

        // 현재 시간과 비교하여 쿨다운이 지났을 때만 실행
        if (Time.time >= nextPickupTime && Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Pick Up Item")))
        {
            GameObject nearestItem = FindNearestItem();
            if (nearestItem != null)
            {
                ItemObject itemPickup = nearestItem.GetComponent<ItemObject>();
                if (itemPickup != null)
                {
                    PickupItem(nearestItem, itemPickup);
                    nextPickupTime = Time.time + pickupCooldown;
                }
            }
        }
    }

    private void PickupItem(GameObject nearestItem, ItemObject itemPickup)
    {
        Inventory inventory = Inventory.instance;

        if (inventory.CanQuickItem())
        {
            if (inventory.quickSlotItems[inventory.selectedQuickSlot] != null && inventory.quickSlotItems[inventory.selectedQuickSlot].data == null)
            {
                inventory.Move_QuickSlot_Item(itemPickup.itemData, inventory.selectedQuickSlot);
                Destroy(nearestItem);

                return;
            }
        }

        if (inventory.isPocket)
        {
            if (inventory.CanAddItem())
            {
                inventory.AddItem(itemPickup.itemData);
                Destroy(nearestItem);

                return;
            }
        }
    }

    // 플레이어 기준 가장 가까운 아이템 찾기
    GameObject FindNearestItem()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject item in items)
        {
            float distance = Vector3.Distance(item.transform.position, currentPos);
            if (distance < pickupRange && distance < minDistance)
            {
                nearest = item;
                minDistance = distance;
            }
        }
        return nearest;
    }
}
