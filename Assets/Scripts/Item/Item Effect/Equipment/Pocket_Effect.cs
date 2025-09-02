using UnityEngine;

[CreateAssetMenu(fileName = "Pocket", menuName = "Data/Item Equipment effect/Pocket Effect")]
public class Pocket_Effect : Equipment_Effect
{
    public override void ExecuteEffect()
    {
        InventoryManager.Instance.isPocket = true;

    }

    public override void UnExecuteEffect()
    {
        InventoryManager.Instance.isPocket = false;
    }
}
