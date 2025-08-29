using UnityEngine;

public enum ItemType
{
    Equipment,
    UseItem,
    Normal,
    Special,
    Natural
}

[CreateAssetMenu(fileName = "New ItemData", menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    public int itemId = -1;
    public ItemType itemType;
    public string itemName;
    public Sprite icon;
    [TextArea(3, 10)]
    public string explanation = "";

    [Header("=== (Inspector) ===")]
    public Vector2Int priceRange = new Vector2Int(50, 100);
    
    [Tooltip("true로 설정하면 ItemSpawner에 의해 맵에 드롭될 수 있습니다.")]
    public bool isSpawnable = true;
}