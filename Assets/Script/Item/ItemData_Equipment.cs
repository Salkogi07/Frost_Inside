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

    public void AddModifiers()
    {

    }

    public void RemoveModifiers()
    {

    }
}
