using UnityEngine;

[CreateAssetMenu(fileName = "Stamina", menuName = "Data/UseItem Effect/Stamina Effect")]
public class Stamina_Effect : UseItem_Effect
{
    public override void ExecuteEffect(Transform _playerPos)
    {
        GameManager.instance.playerPrefab.GetComponent<Player_Condition>().AddStamina(10);
    }
}


