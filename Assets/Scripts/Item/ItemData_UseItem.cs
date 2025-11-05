using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Data/UseItem")]
public class ItemData_UseItem : ItemData
{
    public UseItem_Effect[] itemEffect;

    public void ExecuteItemEffect(GameObject _playerPos)
    {
        foreach (var item in itemEffect)
        {
            item.ExecuteEffect(_playerPos);
        }
    }
}
