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
    public ItemType itemType;
    public string itemName;
    public Sprite icon;
    [TextArea(3, 10)]
    public string explanation = "";

    [Header("=== 가격 범위 설정 (Inspector) ===")]
    public Vector2Int useItemPriceRange = new Vector2Int(100, 180);
    public Vector2Int normalPriceRange = new Vector2Int(40, 70);
    public Vector2Int specialPriceRange = new Vector2Int(80, 130);
    public Vector2Int naturalPriceRange = new Vector2Int(60, 120);
}
