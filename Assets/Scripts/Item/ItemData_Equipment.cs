using UnityEngine;

public enum EquipmentType
{
    Back_Slot,
    Tool
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Data/Equipment")]
public class ItemData_Equipment : ItemData
{
    public EquipmentType equipmentType;

    public Equipment_Effect[] itemEffect;

    public int mining;
    public int armor;
    public int lagging;

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
        Player_Stats playerStats = PlayerManager.instance.playerStats;

        playerStats.mining.AddModifier(mining);
        playerStats.armor.AddModifier(armor);
        playerStats.lagging.AddModifier(lagging);
        playerStats.maxHp.AddModifier(Hp);
        playerStats.maxTemperature.AddModifier(Temperature);
    }

    public void RemoveModifiers()
    {
        Player_Stats playerStats = PlayerManager.instance.playerStats;

        playerStats.mining.RemoveModifier(mining);
        playerStats.armor.RemoveModifier(armor);
        playerStats.lagging.RemoveModifier(lagging);
        playerStats.maxHp.RemoveModifier(Hp);
        playerStats.maxTemperature.RemoveModifier(Temperature);
    }
}
