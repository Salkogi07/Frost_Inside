using UnityEngine;

[CreateAssetMenu(fileName = "Pocket", menuName = "Data/Item Equipment effect/Pocket Effect")]
public class Pocket_Effect : Equipment_Effect
{
    public override void ExecuteEffect()
    {
        InventoryManager.Instance.isPocket = true;
        
        InventoryUI.Instance.UpdatePoketPanel();
        UI_ItemSlot.CancelDrag();
    }

    public override void UnExecuteEffect()
    {
        InventoryManager.Instance.isPocket = false;
        
        if (!InventoryManager.Instance.isPocket)
        {
            GameManager.instance.playerPrefab.GetComponent<Player_ItemDrop>().NotPocket_ItemDrop();
        }
        
        InventoryUI.Instance.UpdatePoketPanel();
        UI_ItemSlot.CancelDrag();
    }
}
