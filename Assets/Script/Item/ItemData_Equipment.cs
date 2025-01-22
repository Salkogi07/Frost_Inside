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
}
