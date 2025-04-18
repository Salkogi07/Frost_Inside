using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class SpreadTilemap : MonoBehaviour
{
    [SerializeField]
    private Tilemap floor;
    [SerializeField]
    private Tilemap wall;
    [SerializeField]
    private Tilemap corridor;  // 통로 타일맵
    [SerializeField]
    private TileBase floorTile;  // 기본 타일 (필요시 사용)
    [SerializeField]
    private TileBase wallTile;   // 기본 타일 (필요시 사용)
    [SerializeField]
    private TileBase corridorTile; // 기본 타일 (필요시 사용)

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

    // 원본 타일 정보를 사용하여 타일맵에 배치
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

    private void SpreadTile(HashSet<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var pos in positions)
        {
            tilemap.SetTile((Vector3Int)pos, tile);
        }
    }

    // 원본 타일 정보를 사용하여 타일 배치
    private void SpreadTileWithOriginal(Dictionary<Vector2Int, TileBase> tileDict, Tilemap tilemap)
    {
        foreach (var pair in tileDict)
        {
            tilemap.SetTile((Vector3Int)pair.Key, pair.Value);
        }
    }

    public void ClearAllTiles()
    {
        floor.ClearAllTiles();
        wall.ClearAllTiles();
        corridor.ClearAllTiles();
    }
}