using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "bomb", menuName = "Data/UseItem Effect/Bomb Effect")]
public class Bomb_Effect : UseItem_Effect
{
    public GameObject boomObj;

    public override void ExecuteEffect(Transform _playerPos)
    {
        GameObject bombInstance = Instantiate(boomObj, _playerPos.position, Quaternion.identity);
        
        bombInstance.GetComponent<NetworkObject>().Spawn();
    }
}
