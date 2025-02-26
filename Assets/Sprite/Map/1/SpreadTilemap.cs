using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class SpreadTilemap : MonoBehaviour
{
    [SerializeField]
    private Tilemap floor;
    [SerializeField]
    private Tilemap wall;
    [SerializeField]
    private Tilemap corridor;  // 통로 타일맵

    [SerializeField]
    private TileBase floorTile;
    [SerializeField]
    private TileBase wallTile;
    [SerializeField]
    private TileBase corridorTile; // 통로 타일

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

    private void SpreadTile(HashSet<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var pos in positions)
        {
            tilemap.SetTile((Vector3Int)pos, tile);
        }
    }

    public void ClearAllTiles()
    {
        floor.ClearAllTiles();
        wall.ClearAllTiles();
        corridor.ClearAllTiles();
    }
}
