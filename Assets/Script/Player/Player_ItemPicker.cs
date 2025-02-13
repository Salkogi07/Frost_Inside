using UnityEngine;

public class Player_ItemPicker : MonoBehaviour
{
    [Tooltip("�������� �ݱ� ���� �ִ� �Ÿ�")]
    public float pickupRange = 3f;

    [Tooltip("������ �ݱ� Ű")]
    public KeyCode pickupKey = KeyCode.F;

    [Tooltip("������ �ݱ� �� ��ٿ� �ð� (��)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    void Update()
    {
        // ���� �ð��� ���Ͽ� ��ٿ��� ������ ���� ����
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

    // �÷��̾� ���� ���� ����� ������ ã��
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
