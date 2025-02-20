using UnityEngine;

public class Player_ItemPicker : MonoBehaviour
{
    Player_Move player_move;

    [Tooltip("�������� �ݱ� ���� �ִ� �Ÿ�")]
    public float pickupRange = 3f;

    [Tooltip("������ �ݱ� �� ��ٿ� �ð� (��)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    private void Awake()
    {
        player_move = GetComponent<Player_Move>();
    }

    void Update()
    {
        if(player_move.isDead)
            return;

        // ���� �ð��� ���Ͽ� ��ٿ��� ������ ���� ����
        if (Time.time >= nextPickupTime && Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Pick Up Item")))
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
