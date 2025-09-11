using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileMapType
{
    Ground,
    Background,
    Wall,
    Ore,
}

public class SpreadTilemap : MonoBehaviour
{
    [Header("=== 기본 타일맵 ===")]
    [SerializeField] private Tilemap ground;
    [SerializeField] private Tilemap background;
    [SerializeField] private Tilemap wall;
    [SerializeField] private Tilemap corridor;
    [SerializeField] private Tilemap itemSpawn;
    [SerializeField] private Tilemap monsterSpawn;
    [SerializeField] private Tilemap initialMonsterSpawn;

    [Header("=== 광석 타일맵 ===")]
    [SerializeField] private Tilemap ore;

    public Tilemap OreTilemap => ore;
    public Tilemap ItemSpawnTilemap => itemSpawn;
    public Tilemap MonsterSpawnTilemap => monsterSpawn;
    public Tilemap InitialMonsterSpawnTilemap => initialMonsterSpawn;

    [Header("=== 기본 타일 ===")]
    [SerializeField] private TileBase backgroundTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase corridorTile;
    [SerializeField] private TileBase itemSpawnTile;
    [SerializeField] private TileBase monsterSpawnTile;
    [SerializeField] private TileBase initialMonsterSpawnTile;

    [Header("=== 지면 노이즈 설정 ===")]
    [Tooltip("지면 타일을 그릴 타일맵")]
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("Perlin 노이즈로 생성할 지면 타일 종류 리스트")]
    [SerializeField] private List<TileBase> groundVariants;
    [Tooltip("노이즈 스케일 (작게 할수록 디테일이 커짐)")]
    [SerializeField, Range(0.01f, 1f)] private float noiseScale = 0.1f;

    [Header("=== 배경 노이즈 설정 ===")]
    [Tooltip("배경에 사용할 지면 타일맵")]
    [SerializeField] private Tilemap backgroundTilemap;
    [Tooltip("배경의 색상(기본값: 80% 밝기)")]
    [SerializeField] private Color backgroundTint = new Color(0.8f, 0.8f, 0.8f, 1f);

    // 기본 타일 퍼뜨리기
    public void SpreadBackgroundTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, background, backgroundTile);
    }

    public void SpreadWallTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, wall, wallTile);
    }

    public void SpreadCorridorTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, corridor, corridorTile);
    }

    public void SpreadItemSpawnTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, itemSpawn, itemSpawnTile);
    }

    public void SpreadMonsterSpawnTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, monsterSpawn, monsterSpawnTile);
    }
    
    public void SpreadInitialMonsterSpawnTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, initialMonsterSpawn, initialMonsterSpawnTile);
    }

    // 직접 Tile 지정해서 퍼뜨리기
    public void SpreadBackgroundTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, background);
    }

    public void SpreadWallTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, wall);
    }

    public void SpreadCorridorTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, corridor);
    }

    public void SpreadItemSpawnTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, itemSpawn);
    }

    public void SpreadMonsterSpawnTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, monsterSpawn);
    }
    
    public void SpreadInitialMonsterSpawnTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, initialMonsterSpawn);
    }

    // 모든 타일맵 초기화
    public void ClearAllTiles()
    {
        background.ClearAllTiles();
        wall.ClearAllTiles();
        corridor.ClearAllTiles();
        itemSpawn.ClearAllTiles();
        if (groundTilemap != null) groundTilemap.ClearAllTiles();
        if (monsterSpawn != null) monsterSpawn.ClearAllTiles();
        if (initialMonsterSpawn != null) initialMonsterSpawn.ClearAllTiles();
    }

    /// <summary>
    /// 복도 타일맵의 렌더러를 비활성화합니다.
    /// </summary>
    public void HideCorridorRenderer()
    {
        var rend = corridor.GetComponent<TilemapRenderer>();
        if (rend != null)
            rend.enabled = false;
    }

    /// <summary>
    /// 아이템 스폰 타일맵의 렌더러를 비활성화합니다.
    /// </summary>
    public void HideItemSpawnRenderer()
    {
        var rend = itemSpawn.GetComponent<TilemapRenderer>();
        if (rend != null)
            rend.enabled = false;
    }

    /// <summary>
    /// 몬스터 스폰 타일맵의 렌더러를 비활성화합니다.
    /// </summary>
    public void HideMonsterSpawnRenderer()
    {
        var rend = monsterSpawn.GetComponent<TilemapRenderer>();
        if (rend != null)
            rend.enabled = false;
    }
    
    /// <summary>
    /// 초기 몬스터 스폰 타일맵의 렌더러를 비활성화합니다.
    /// </summary>
    public void HideInitialMonsterSpawnRenderer()
    {
        var rend = initialMonsterSpawn.GetComponent<TilemapRenderer>();
        if (rend != null)
            rend.enabled = false;
    }

    /// <summary>
    /// 주어진 구역(mapMin~mapMax)에서 floor와 wall이 아닌 타일 위치에
    /// Perlin 노이즈를 활용해 variants 중 하나의 타일을 설정합니다.
    /// 배경 타일에는 지정된 색상을 적용합니다.
    /// </summary>
    public void FillGroundWithNoise(
        Vector2Int mapMin,
        Vector2Int mapMax,
        HashSet<Vector2Int> floorTiles,
        HashSet<Vector2Int> wallTiles,
        int seed
    )
    {
        // 1) x, y에 씨드(Seed) 오프셋 추가
        float seedOffsetX = (seed % 10000) * 0.0001f;
        float seedOffsetY = ((seed / 10000) % 10000) * 0.0001f;

        // 2) 랜덤 각도 설정(0~359도)
        float angle = (seed % 360) * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        for (int x = mapMin.x; x <= mapMax.x; x++)
        {
            for (int y = mapMin.y; y <= mapMax.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // floor 또는 wall이면 건너뜀
                if (floorTiles.Contains(pos) || wallTiles.Contains(pos))
                    continue;

                // 좌표 변환 + 노이즈 적용
                float nx = (x * cosAngle - y * sinAngle) * noiseScale + seedOffsetX;
                float ny = (x * sinAngle + y * cosAngle) * noiseScale + seedOffsetY;

                // Perlin 노이즈 값
                float n = Mathf.PerlinNoise(nx, ny);

                // variants에서 타일 선택
                int idx = Mathf.Clamp(
                    Mathf.FloorToInt(n * groundVariants.Count),
                    0,
                    groundVariants.Count - 1
                );

                Vector3Int cellPos = (Vector3Int)pos;

                // 1) 지면 타일 적용
                groundTilemap.SetTile(cellPos, groundVariants[idx]);

                // 2) 배경 타일 및 색상 적용
                if (backgroundTilemap != null)
                {
                    backgroundTilemap.SetTile(cellPos, groundVariants[idx]);
                    backgroundTilemap.SetTileFlags(cellPos, TileFlags.None);
                    backgroundTilemap.SetColor(cellPos, backgroundTint);
                }
            }
        }
    }
    
    private void SpreadTile(HashSet<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var pos in positions)
            tilemap.SetTile((Vector3Int)pos, tile);
    }
    
    private void SpreadTileWithOriginal(Dictionary<Vector2Int, TileBase> tileDict, Tilemap tilemap)
    {
        foreach (var pair in tileDict)
            tilemap.SetTile((Vector3Int)pair.Key, pair.Value);
    }

    public TileMapType GetTileMapType(Tilemap tilemap)
    {
        switch (tilemap.name)
        {
            case "Ground":
                return TileMapType.Ground;
            case "Background":
                return TileMapType.Background;
            case "Wall":
                return TileMapType.Wall;
            case "Ore":
                return TileMapType.Ore;
            default:
                return TileMapType.Ground;
        }
    }

    public Tilemap GetTileMap(TileMapType tileMapType)
    {
        switch (tileMapType)
        {
            case TileMapType.Ground:
                return ground;
            case TileMapType.Background:
                return background;
            case TileMapType.Wall:
                return wall;
            case TileMapType.Ore:
                return ore;
            default:
                return ground;
        }
    }
}