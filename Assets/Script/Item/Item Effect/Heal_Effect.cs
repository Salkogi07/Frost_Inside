using UnityEngine;

[CreateAssetMenu(fileName = "Heal", menuName = "Data/Item effect/Heal Effect")]
public class Heal_Effect : ItemEffect
{
    [SerializeField] private GameObject healEffect;

    public override void ExecuteEffect(Transform _playerPosition)
    {
        GameObject newHealEffect = Instantiate(healEffect, _playerPosition.position, Quaternion.identity);
    }
}
