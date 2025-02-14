using UnityEngine;

public class Player_ItemPicker : MonoBehaviour
{
    [Tooltip("아이템을 줍기 위한 최대 거리")]
    public float pickupRange = 3f;

    [Tooltip("아이템 줍기 키")]
    public KeyCode pickupKey = KeyCode.E;

    [Tooltip("아이템 줍기 후 쿨다운 시간 (초)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    void Update()
    {
        // 현재 시간과 비교하여 쿨다운이 지났을 때만 실행
        if (Time.time >= nextPickupTime && Input.GetKeyDown(pickupKey))
        {
            GameObject nearestItem = FindNearestItem();
            if (nearestItem != null)
            {
                ItemObject itemPickup = nearestItem.GetComponent<ItemObject>();
                if (itemPickup != null)
                {
                    Inventory.instance.AddItem(itemPickup.itemData);
                    Destroy(nearestItem);
                    nextPickupTime = Time.time + pickupCooldown;
                }
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
