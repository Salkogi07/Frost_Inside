using UnityEngine;

public class Player_UseItem : MonoBehaviour
{
    Transform playerPos;

    private void Start()
    {
        playerPos = GetComponent<Transform>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Use Item")))
        {
            Inventory inventory = Inventory.instance;
            if (inventory.quickSlotItems[inventory.selectedQuickSlot]?.data != null)
            {
                ItemData_UseItem item = inventory.quickSlotItems[inventory.selectedQuickSlot]?.data as ItemData_UseItem;
                if (item != null)
                {
                    item?.ExecuteItemEffect(playerPos);
                    inventory.quickSlotItems[inventory.selectedQuickSlot] = null;
                }
            }
        }
    }
}
