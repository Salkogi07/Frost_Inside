using UnityEngine;

[CreateAssetMenu(fileName = "Pocket", menuName = "Data/Item Equipment effect/Pocket Effect")]
public class Pocket_Effect : Equipment_Effect
{
    public override void ExecuteEffect()
    {
        Inventory.instance.isPocket = true;
        UIManager.instance.UpdatePocket();
    }

    public override void UnExecuteEffect()
    {
        Inventory.instance.isPocket = false;
        UIManager.instance.UpdatePocket();
    }
}
