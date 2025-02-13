using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class SpreadTilemap : MonoBehaviour
{
    [SerializeField]
    private Tilemap floor;
    [SerializeField]
    private Tilemap wall;
    [SerializeField]
    private TileBase floorTile;
    [SerializeField]
    private TileBase wallTile;

    public void SpreadFloorTilemap (HashSet<Vector2Int> position)
    {
        SpreadTile(position, floor, floorTile);
    }

    public void SpreadWallTilemap(HashSet<Vector2Int> position)
    {
        SpreadTile(position, wall, wallTile);
    }

    private void SpreadTile(HashSet<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach(var position in positions)
        {
            tilemap.SetTile((Vector3Int)position, tile);
        }
    }

    public void ClearAllTiles()
    {
        floor.ClearAllTiles();
        wall.ClearAllTiles();
    }
}
