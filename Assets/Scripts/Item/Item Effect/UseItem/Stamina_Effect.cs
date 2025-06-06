using UnityEngine;

[CreateAssetMenu(fileName = "Stamina", menuName = "Data/UseItem Effect/Stamina Effect")]
public class Stamina_Effect : UseItem_Effect
{
    public override void ExecuteEffect(Transform _playerPos)
    {
        //PlayerManager.instance.playerStats.AddStamina(10);
    }
}


