using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpreadTilemap : MonoBehaviour
{
    [Header("=== 기존 Tilemap ===")]
    [SerializeField] private Tilemap floor;
    [SerializeField] private Tilemap wall;
    [SerializeField] private Tilemap corridor;

    [Header("=== 기본 Tile ===")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase corridorTile;

    [Header("=== Ground Tilemap ===")]
    [Tooltip("방·벽 제외한 영역에 채울 Tilemap")]
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("방·벽 제외한 빈 공간에 채울 타일")]
    [SerializeField] private TileBase groundTile;

    // 단일 Tile 스프레드
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

    // 원본 타일(Dictionary)로 스프레드
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

    // 모든 Tilemap 초기화
    public void ClearAllTiles()
    {
        floor.ClearAllTiles();
        wall.ClearAllTiles();
        corridor.ClearAllTiles();
        if (groundTilemap != null)
            groundTilemap.ClearAllTiles();
    }

    /// <summary>
    /// Corridor용 TilemapRenderer를 비활성화
    /// </summary>
    public void HideCorridorRenderer()
    {
        var rend = corridor.GetComponent<TilemapRenderer>();
        if (rend != null)
            rend.enabled = false;  // TilemapRenderer.enabled 사용 :contentReference[oaicite:0]{index=0}
    }

    /// <summary>
    /// mapMin~mapMax 범위 내에서 floor·wall 제외한 영역을 groundTile로 채운다.
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

    // 내부 헬퍼: 단일 타일 스프레드
    private void SpreadTile(HashSet<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var pos in positions)
            tilemap.SetTile((Vector3Int)pos, tile);  // Tilemap.SetTile API :contentReference[oaicite:2]{index=2}
    }

    // 내부 헬퍼: 원본 TileDictionary 스프레드
    private void SpreadTileWithOriginal(Dictionary<Vector2Int, TileBase> tileDict, Tilemap tilemap)
    {
        foreach (var pair in tileDict)
            tilemap.SetTile((Vector3Int)pair.Key, pair.Value);  // Tilemap.SetTile API :contentReference[oaicite:3]{index=3}
    }
}
