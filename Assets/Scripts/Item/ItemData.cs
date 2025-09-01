using UnityEngine;
using UnityEngine.Tilemaps;

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
    
    [Header("광석 전용 설정")]
    [Tooltip("이 아이템이 맵에 생성되는 광석인 경우 체크합니다.")]
    public bool isOre = false;
    [Tooltip("광석인 경우, 맵에 표시될 타일 프리팹을 할당합니다.")]
    public TileBase oreTile;
    
    [Header("맵생성 전용 설정")]
    [Tooltip("true로 설정하면 ItemSpawner에 의해 맵에 드롭될 수 있습니다.")]
    public bool isSpawnable = true;
}