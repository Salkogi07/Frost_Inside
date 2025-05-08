using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpreadTilemap : MonoBehaviour
{
    [Header("=== ���� Tilemap ===")]
    [SerializeField] private Tilemap floor;
    [SerializeField] private Tilemap wall;
    [SerializeField] private Tilemap corridor;
    [SerializeField] private Tilemap itemSpawn;

    [Header("=== �⺻ Tile ===")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase corridorTile;
    [SerializeField] private TileBase itemSpawnTile;

    [Header("=== Ground Noise ���� ===")]
    [Tooltip("�桤�� ������ ������ ä�� Tilemap")]
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("Perlin Noise�� ä�� Ground Ÿ�� ����")]
    [SerializeField] private List<TileBase> groundVariants;
    [Tooltip("Noise �� (�۰� �Ҽ��� ���� ��ġ)")]
    [SerializeField, Range(0.01f, 1f)] private float noiseScale = 0.1f;

    [Header("=== Background Noise ���� ===")]
    [Tooltip("��ο� ������ Ground�� �� Tilemap")]
    [SerializeField] private Tilemap backgroundTilemap;
    [Tooltip("��Ӱ� ó���� ���� (�⺻ 80% ���)")]
    [SerializeField] private Color backgroundTint = new Color(0.8f, 0.8f, 0.8f, 1f);

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

    public void SpreadItemSpawnTilemap(HashSet<Vector2Int> positions)
    {
        SpreadTile(positions, itemSpawn, itemSpawnTile);
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

    public void SpreadItemSpawnTilemapWithTiles(Dictionary<Vector2Int, TileBase> tileDict)
    {
        SpreadTileWithOriginal(tileDict, itemSpawn);
    }


    // ��� Tilemap �ʱ�ȭ
    public void ClearAllTiles()
    {
        floor.ClearAllTiles();
        wall.ClearAllTiles();
        corridor.ClearAllTiles();
        itemSpawn.ClearAllTiles();
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
    /// ItemSpawn�� TilemapRenderer�� ��Ȱ��ȭ
    /// </summary>
    public void HideItemSpawnRenderer()
    {
        var rend = itemSpawn.GetComponent<TilemapRenderer>();
        if (rend != null)
            rend.enabled = false;  // TilemapRenderer.enabled ��� :contentReference[oaicite:0]{index=0}
    }

    /// <summary>
    /// mapMin~mapMax ���� ������ floor��wall ������ ������
    /// Perlin Noise ������� variants �� �ϳ��� ��� ä��ϴ�.
    /// �¿졤���� ��Ī ������ ��ȭ�ϱ� ���� �Է� ��ǥ�� ȸ���ϰ�
    /// x, y �࿡ ���� �ٸ� �õ� �������� �����մϴ�.
    /// </summary>
    public void FillGroundWithNoise(
        Vector2Int mapMin,
        Vector2Int mapMax,
        HashSet<Vector2Int> floorTiles,
        HashSet<Vector2Int> wallTiles,
        int seed
    )
    {
        // 1) x, y�� ������ �и� (seed ���� �ٸ��� ���)
        float seedOffsetX = (seed % 10000) * 0.0001f;
        float seedOffsetY = ((seed / 10000) % 10000) * 0.0001f;

        // 2) ���� ������ �Է� ��ǥ ȸ�� (0~359��)
        float angle = (seed % 360) * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        for (int x = mapMin.x; x <= mapMax.x; x++)
        {
            for (int y = mapMin.y; y <= mapMax.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // floor �Ǵ� wall Ÿ���� �ǳʶ�
                if (floorTiles.Contains(pos) || wallTiles.Contains(pos))
                    continue;

                // ȸ���� ��ǥ + ������ + �ະ ������
                float nx = (x * cosAngle - y * sinAngle) * noiseScale + seedOffsetX;
                float ny = (x * sinAngle + y * cosAngle) * noiseScale + seedOffsetY;

                // Perlin Noise ���ø�
                float n = Mathf.PerlinNoise(nx, ny);

                // variants �� �ε��� ����
                int idx = Mathf.Clamp(
                    Mathf.FloorToInt(n * groundVariants.Count),
                    0,
                    groundVariants.Count - 1
                );

                Vector3Int cellPos = (Vector3Int)pos;

                // 1) Ground Ÿ�� ��ġ
                groundTilemap.SetTile(cellPos, groundVariants[idx]);

                // 2) Background Ÿ�Ͽ� ��Ӱ� ó��
                if (backgroundTilemap != null)
                {
                    backgroundTilemap.SetTile(cellPos, groundVariants[idx]);
                    backgroundTilemap.SetTileFlags(cellPos, TileFlags.None);
                    backgroundTilemap.SetColor(cellPos, backgroundTint);
                }
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
