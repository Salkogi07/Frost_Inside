using Unity.Netcode;
using UnityEngine;

public enum EquipmentType
{
    Back_Slot,
    Shoulder_Slot,
    Tool_Slot
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Data/Equipment")]
public class ItemData_Equipment : ItemData
{
    public EquipmentType equipmentType;

    public Equipment_Effect[] itemEffect;

    public int mining;
    public int armor;
    public int lagging;
    public int damage;

    public int Hp;
    public int Temperature;

    public void ExecuteItemEffect()
    {
        foreach (var item in itemEffect)
        {
            item.ExecuteEffect();
        }
    }

    public void UnExecuteItemEffect()
    {
        foreach (var item in itemEffect)
        {
            item.UnExecuteEffect();
        }
    }

    public void AddModifiers()
    {
        GameObject playerObj = GameManager.instance.playerPrefab;
        Player_Stats playerStats = playerObj.GetComponent<Player_Stats>();

        playerStats.Mining.AddModifier(mining);
        playerStats.Armor.AddModifier(armor);
        playerStats.Lagging.AddModifier(lagging);
        playerStats.Damage.AddModifier(damage);
        playerStats.MaxHp.AddModifier(Hp);
        playerStats.MaxTemperature.AddModifier(Temperature);
    }

    public void RemoveModifiers()
    {
        GameObject playerObj = GameManager.instance.playerPrefab;
        Player_Stats playerStats = playerObj.GetComponent<Player_Stats>();

        playerStats.Mining.RemoveModifier(mining);
        playerStats.Armor.RemoveModifier(armor);
        playerStats.Lagging.RemoveModifier(lagging);
        playerStats.Damage.RemoveModifier(damage);
        playerStats.MaxHp.RemoveModifier(Hp);
        playerStats.MaxTemperature.RemoveModifier(Temperature);
    }
}
