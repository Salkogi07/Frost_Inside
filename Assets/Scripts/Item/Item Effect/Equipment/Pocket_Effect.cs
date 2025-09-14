using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Pocket", menuName = "Data/Item Equipment effect/Pocket Effect")]
public class Pocket_Effect : Equipment_Effect
{
    public override void ExecuteEffect()
    {
        InventoryManager.instance.isPocket = true;
        
        InventoryUI.Instance.UpdatePoketPanel();
        UI_ItemSlot.CancelDrag();
    }

    public override void UnExecuteEffect()
    {
        InventoryManager.instance.isPocket = false;
        
        if (!InventoryManager.instance.isPocket)
        {
            GameObject playerObj = GameManager.instance.playerPrefab;
            playerObj.GetComponent<Player_ItemDrop>().NotPocket_ItemDrop();
        }
        
        InventoryUI.Instance.UpdatePoketPanel();
        UI_ItemSlot.CancelDrag();
    }
}
