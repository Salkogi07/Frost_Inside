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

    [Header("=== Background Noise 설정 ===")]
    [Tooltip("어두운 버전의 Ground를 깔 Tilemap")]
    [SerializeField] private Tilemap backgroundTilemap;
    [Tooltip("어둡게 처리할 색상 (기본 80% 밝기)")]
    [SerializeField] private Color backgroundTint = new Color(0.8f, 0.8f, 0.8f, 1f);

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
        // seed를 기반으로 Perlin Noise 오프셋 계산
        float seedOffset = (seed % 10000) * 0.0001f;

        for (int x = mapMin.x; x <= mapMax.x; x++)
        {
            for (int y = mapMin.y; y <= mapMax.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // 방(floor)이나 벽(wall) 타일은 건너뜀
                if (floorTiles.Contains(pos) || wallTiles.Contains(pos))
                    continue;

                // Perlin Noise 샘플링
                float sampleX = x * noiseScale + seedOffset;
                float sampleY = y * noiseScale + seedOffset;
                float n = Mathf.PerlinNoise(sampleX, sampleY);

                // groundVariants 리스트 중 하나를 선택
                int idx = Mathf.Clamp(
                    Mathf.FloorToInt(n * groundVariants.Count),
                    0,
                    groundVariants.Count - 1
                );

                Vector3Int cellPos = (Vector3Int)pos;

                // 1) Ground 타일맵에 기본 타일 배치
                groundTilemap.SetTile(cellPos, groundVariants[idx]);

                // 2) Background 타일맵에 동일한 타일을 어둡게 배치
                if (backgroundTilemap != null)
                {
                    backgroundTilemap.SetTile(cellPos, groundVariants[idx]);
                    // 색상 변경을 위해 Lock 해제
                    backgroundTilemap.SetTileFlags(cellPos, TileFlags.None);
                    // 어두운 색조 적용
                    backgroundTilemap.SetColor(cellPos, backgroundTint);
                }
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
