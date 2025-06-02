using UnityEngine.Tilemaps;

[System.Serializable]
public class OreSetting
{
    public TileBase oreTile;    // 광석 타일
    public InventoryItem dropItem;   // 채굴 시 드롭할 인벤토리 아이템 (가격 포함)
}