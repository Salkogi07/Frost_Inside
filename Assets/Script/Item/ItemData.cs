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

    [Header("=== ���� ���� ���� (Inspector) ===")]
    public Vector2Int normalPriceRange = new Vector2Int(50, 80);
    public Vector2Int specialPriceRange = new Vector2Int(100, 150);
    public Vector2Int naturalPriceRange = new Vector2Int(80, 130);

    public int Price { get; private set; }

    private void OnEnable()
    {
        Price = GenerateRandomPrice();
    }

    private int GenerateRandomPrice()
    {
        Vector2Int range = itemType switch
        {
            ItemType.Normal => normalPriceRange,
            ItemType.Special => specialPriceRange,
            ItemType.Natural => naturalPriceRange,
            _ => new Vector2Int(0, 0)
        };
        // int �����ε�: max�� exclusive �� max+1 �� �Ѱܾ� max�� ���Ե˴ϴ�.
        return Random.Range(range.x, range.y + 1);  // :contentReference[oaicite:0]{index=0}
    }
}
