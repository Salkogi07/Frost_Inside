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
    private Tilemap corridor;  // ��� Ÿ�ϸ�
    [SerializeField]
    private TileBase floorTile;  // �⺻ Ÿ�� (�ʿ�� ���)
    [SerializeField]
    private TileBase wallTile;   // �⺻ Ÿ�� (�ʿ�� ���)
    [SerializeField]
    private TileBase corridorTile; // �⺻ Ÿ�� (�ʿ�� ���)

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

    // ���� Ÿ�� ������ ����Ͽ� Ÿ�ϸʿ� ��ġ
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

    // ���� Ÿ�� ������ ����Ͽ� Ÿ�� ��ġ
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