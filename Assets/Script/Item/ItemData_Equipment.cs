using UnityEngine;

public enum EquipmentType
{
    Main_Hand,
    Serve_Hand,
    Armor
}

[CreateAssetMenu(fileName = "New ItemDatat", menuName = "Data/Equipment")]
public class ItemData_Equipment : ItemData
{
    public EquipmentType equipmentType;

    public int mining;
    public int armor;
    public int lagging;

    public int Hp;
    public int Temperature;

    public void AddModifiers()
    {
        Player_Stats playerStats = PlayerManager.instance.player;

        playerStats.mining.AddModifier(mining);
        playerStats.armor.AddModifier(armor);
        playerStats.lagging.AddModifier(lagging);
        playerStats.maxHp.AddModifier(Hp);
        playerStats.maxTemperature.AddModifier(Temperature);
    }

    public void RemoveModifiers()
    {
        Player_Stats playerStats = PlayerManager.instance.player;

        playerStats.mining.RemoveModifier(mining);
        playerStats.armor.RemoveModifier(armor);
        playerStats.lagging.RemoveModifier(lagging);
        playerStats.maxHp.RemoveModifier(Hp);
        playerStats.maxTemperature.RemoveModifier(Temperature);
    }
}
