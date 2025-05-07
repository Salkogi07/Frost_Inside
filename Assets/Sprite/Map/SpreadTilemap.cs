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
    
    [Header("=== Ground Noise 설정 ===")]
    [Tooltip("방·벽 제외한 영역에 채울 Tilemap")]
    [SerializeField] private Tilemap groundTilemap;

    [Tooltip("Perlin Noise로 채울 Ground 타일 종류")]
    [SerializeField] private List<TileBase> groundVariants;
    [Tooltip("Noise 빈도 (작게 할수록 넓은 패치)")]
    [SerializeField, Range(0.01f, 1f)] private float noiseScale = 0.1f;

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
    /// mapMin~mapMax 범위 내에서 floor·wall 제외한 영역을
    /// Perlin Noise 기반으로 variants 중 하나를 골라 채웁니다.
    /// </summary>
    public void FillGroundWithNoise(
        Vector2Int mapMin,
        Vector2Int mapMax,
        HashSet<Vector2Int> floorTiles,
        HashSet<Vector2Int> wallTiles,
        int seed
    )
    {
        // 0.0000~0.9999 구간의 소수 오프셋 생성
        float seedOffset = (seed % 10000) * 0.0001f;

        for (int x = mapMin.x; x <= mapMax.x; x++)
        {
            for (int y = mapMin.y; y <= mapMax.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (floorTiles.Contains(pos) || wallTiles.Contains(pos))
                    continue;

                // 샘플 좌표 계산
                float sampleX = x * noiseScale + seedOffset;
                float sampleY = y * noiseScale + seedOffset;
                float n = Mathf.PerlinNoise(sampleX, sampleY);

                // 노이즈 값에 따라 variants 인덱스 선택
                int idx = Mathf.FloorToInt(n * groundVariants.Count);
                idx = Mathf.Clamp(idx, 0, groundVariants.Count - 1);

                groundTilemap.SetTile((Vector3Int)pos, groundVariants[idx]);
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
