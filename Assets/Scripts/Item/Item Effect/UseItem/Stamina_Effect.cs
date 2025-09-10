using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Stamina", menuName = "Data/UseItem Effect/Stamina Effect")]
public class Stamina_Effect : UseItem_Effect
{
    public override void ExecuteEffect(Transform _playerPos)
    {
        GameObject playerObj = PlayerDataManager.instance.GetPlayerObject(NetworkManager.Singleton.LocalClientId);
        playerObj.GetComponent<Player_Condition>().AddStamina(10);
    }
}


