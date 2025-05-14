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

    [Header("=== 개별 아이템 가격 범위 설정 (Inspector) ===")]
    [Tooltip("이 아이템의 가격이 랜덤으로 결정될 범위를 지정하세요.")]
    public Vector2Int priceRange = new Vector2Int(50, 100);
}
