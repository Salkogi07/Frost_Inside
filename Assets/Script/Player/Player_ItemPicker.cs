using Unity.VisualScripting;
using UnityEngine;

public class Player_ItemPicker : MonoBehaviour
{
    Player_Move player_move;
    Player_Stats stats;

    [Tooltip("�������� �ݱ� ���� �ִ� �Ÿ�")]
    public float pickupRange = 3f;

    [Tooltip("������ �ݱ� �� ��ٿ� �ð� (��)")]
    public float pickupCooldown = 0.5f;

    private float nextPickupTime = 0f;

    private void Awake()
    {
        player_move = GetComponent<Player_Move>();
        stats = GetComponent<Player_Stats>();
    }

    void Update()
    {
        if(stats.isDead || Inventory.instance.isInvenOpen)
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
            if (inventory.GetCheck_QuiSlot_Item())
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
