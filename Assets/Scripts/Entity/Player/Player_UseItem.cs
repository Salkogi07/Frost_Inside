using UnityEngine;

public class Player_UseItem : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Use Item")))
        {
            InventoryManager.instance.UseItemInQuickSlot();
        }
    }
}
