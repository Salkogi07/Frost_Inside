using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpreadTilemap : MonoBehaviour
{
    [Header("=== ���� Tilemap ===")]
    [SerializeField] private Tilemap floor;
    [SerializeField] private Tilemap wall;
    [SerializeField] private Tilemap corridor;

    [Header("=== �⺻ Tile ===")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase corridorTile;

    [Header("=== Ground Tilemap ===")]
    [Tooltip("�桤�� ������ ������ ä�� Tilemap")]
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("�桤�� ������ �� ������ ä�� Ÿ��")]
    [SerializeField] private TileBase groundTile;

    // ���� Tile ��������
    public void SpreadFloorTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, floor, floorTile);
    }

    public void SpreadWallTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, wall, wallTile);
    }

    public void SpreadCorridorTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, corridor, corridorTile);
    }

    // ���� Ÿ��(Dictionary)�� ��������
    public void SpreadFloorTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, floor);
    }

    public void SpreadWallTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, wall);
    }

    public void SpreadCorridorTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, corridor);
    }

    // ��� Tilemap �ʱ�ȭ
    public void ClearAllTiles()
    {
        floor.ClearAllTiles();
        wall.ClearAllTiles();
        corridor.ClearAllTiles();
        if (groundTilemap != null)
            groundTilemap.ClearAllTiles();
    }

    /// <summary>
    /// Corridor�� TilemapRenderer�� ��Ȱ��ȭ
    /// </summary>
    public void HideCorridorRenderer()
    {
        var rend = corridor.GetComponent<TilemapRenderer>();
        if (rend != null)
            rend.enabled = false;  // TilemapRenderer.enabled ��� :contentReference[oaicite:0]{index=0}
    }

    /// <summary>
    /// mapMin~mapMax ���� ������ floor��wall ������ ������ groundTile�� ä���.
    /// </summary>
    public void FillGroundWithinBounds(Vector2Int mapMin, Vector2Int mapMax,
                                       HashSet<Vector2Int> floorTiles,
                                       HashSet<Vector2Int> wallTiles)
    {
        for (int x = mapMin.x; x <= mapMax.x; x++)
        {
            for (int y = mapMin.y; y <= mapMax.y; y++)
            {
                var pos = new Vector2Int(x, y);
                if (floorTiles.Contains(pos) || wallTiles.Contains(pos))
                    continue;
                groundTilemap.SetTile((Vector3Int)pos, groundTile);  // Tilemap.SetTile API :contentReference[oaicite:1]{index=1}
            }
        }
    }

    // ���� ����: ���� Ÿ�� ��������
    private void SpreadTile(HashSet<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var pos in positions)
            tilemap.SetTile((Vector3Int)pos, tile);  // Tilemap.SetTile API :contentReference[oaicite:2]{index=2}
    }

    // ���� ����: ���� TileDictionary ��������
    private void SpreadTileWithOriginal(Dictionary<Vector2Int, TileBase> tileDict, Tilemap tilemap)
    {
        foreach (var pair in tileDict)
            tilemap.SetTile((Vector3Int)pair.Key, pair.Value);  // Tilemap.SetTile API :contentReference[oaicite:3]{index=3}
    }
}
