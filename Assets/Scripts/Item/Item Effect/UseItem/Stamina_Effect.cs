using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Stamina", menuName = "Data/UseItem Effect/Stamina Effect")]
public class Stamina_Effect : UseItem_Effect
{
    public override void ExecuteEffect(GameObject _playerPos)
    {
        GameObject playerObj = GameManager.instance.playerPrefab;
        playerObj.GetComponent<Player_Condition>().AddStamina(10);
    }
}


