using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MakeRandomMap : MonoBehaviour
{
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private int maxRooms = 5;
    [SerializeField] private SpreadTilemap spreadTilemap;
    [SerializeField] private GameObject player;

    [SerializeField] private int seed;
    [SerializeField] private bool useRandomSeed = true;

    private HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();

    private Dictionary<Vector2Int, TileBase> floorTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> wallTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> corridorTileDict = new Dictionary<Vector2Int, TileBase>();

    private void Start()
    {
        if (useRandomSeed)
            seed = System.Environment.TickCount;
        Random.InitState(seed);
        Debug.Log("Using seed: " + seed);

        GenerateMap();
    }

    public void GenerateMap()
    {
        spreadTilemap.ClearAllTiles();
        floorTiles.Clear();
        wallTiles.Clear();
        corridorTiles.Clear();
        floorTileDict.Clear();
        wallTileDict.Clear();
        corridorTileDict.Clear();

        // 첫 방 배치
        PlaceRoom(roomPrefabs[0], Vector2Int.zero);

        // 이후 방 배치
        for (int i = 1; i < maxRooms; i++)
        {
            GameObject nextRoom = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            if (TryFindPlacementForRoom(nextRoom, out Vector2Int offset, out List<Vector2Int> connPoints))
            {
                // 신규 방 추가
                PlaceRoom(nextRoom, offset);

                // 양쪽 벽 삭제
                foreach (var pos in connPoints)
                    RemoveWallAtPosition(pos);
            }
            else
            {
                Debug.Log("더 이상 배치 불가하여 중단");
                break;
            }
        }

        // 최종 타일맵 반영
        spreadTilemap.SpreadFloorTilemapWithTiles(floorTileDict);
        spreadTilemap.SpreadCorridorTilemapWithTiles(corridorTileDict);
        spreadTilemap.SpreadWallTilemapWithTiles(wallTileDict);

        // 플레이어 위치 초기화
        player.transform.position = Vector3.zero;
    }

    private void PlaceRoom(GameObject roomPrefab, Vector2Int offset)
    {
        GetTilemaps(roomPrefab, out Tilemap floorTM, out Tilemap wallTM, out Tilemap corridorTM);

        CopyTilemapWithTiles(floorTM, offset, floorTiles, floorTileDict);
        CopyTilemapWithTiles(wallTM, offset, wallTiles, wallTileDict);
        CopyTilemapWithTiles(corridorTM, offset, corridorTiles, corridorTileDict);
    }

    private bool TryFindPlacementForRoom(
        GameObject roomPrefab,
        out Vector2Int foundOffset,
        out List<Vector2Int> connectionPoints)
    {
        foundOffset = Vector2Int.zero;
        connectionPoints = new List<Vector2Int>();

        GetTilemaps(roomPrefab, out Tilemap floorTM, out Tilemap wallTM, out Tilemap corridorTM);
        List<Vector2Int> newRoomCorridors = GetLocalCorridorPositions(corridorTM);
        if (newRoomCorridors.Count == 0) return false;

        List<Vector2Int> existingCorridors = new List<Vector2Int>(corridorTiles);
        if (existingCorridors.Count == 0)
            existingCorridors.Add(Vector2Int.zero);

        existingCorridors.Shuffle();
        newRoomCorridors.Shuffle();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var exist in existingCorridors)
        {
            foreach (var local in newRoomCorridors)
            {
                foreach (var dir in directions)
                {
                    int req = (dir.y != 0) ? 1 : 2;
                    Vector2Int offset = (exist + dir) - local;

                    if (OverlapsExistingTiles(floorTM, offset, floorTiles)) continue;
                    if (OverlapsExistingTiles(wallTM, offset, wallTiles)) continue;

                    int connCount = CountCorridorConnections(newRoomCorridors, offset, existingCorridors);
                    if (connCount < req) continue;

                    if (dir.y == 0)
                    {
                        int pairCount = CountCorridorPairConnections(newRoomCorridors, offset, existingCorridors);
                        if (pairCount == 0) continue;
                    }

                    // 연결 지점 계산
                    var points = FindConnectionPoints(newRoomCorridors, offset, existingCorridors);
                    if (points.Count > 0)
                    {
                        foundOffset = offset;
                        connectionPoints = points;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private List<Vector2Int> FindConnectionPoints(
        List<Vector2Int> newLocal,
        Vector2Int offset,
        List<Vector2Int> existing)
    {
        var result = new List<Vector2Int>();
        var existingSet = new HashSet<Vector2Int>(existing);
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var local in newLocal)
        {
            Vector2Int world = local + offset;
            foreach (var d in dirs)
            {
                var neigh = world + d;
                if (existingSet.Contains(neigh))
                {
                    // 벽은 중간 좌표에 위치
                    Vector2Int wall1 = world + (d / 2);
                    Vector2Int wall2 = neigh - (d / 2);
                    result.Add(wall1);
                    result.Add(wall2);
                }
            }
        }
        return result;
    }

    private void RemoveWallAtPosition(Vector2Int pos)
    {
        if (wallTiles.Remove(pos))
        {
            wallTileDict.Remove(pos);
        }
    }

    private List<Vector2Int> GetLocalCorridorPositions(Tilemap tm)
    {
        var list = new List<Vector2Int>();
        foreach (var p in tm.cellBounds.allPositionsWithin)
            if (tm.HasTile(p))
                list.Add((Vector2Int)p);
        return list;
    }

    private int CountCorridorConnections(
        List<Vector2Int> newLocal,
        Vector2Int offset,
        List<Vector2Int> existing)
    {
        int count = 0;
        var existSet = new HashSet<Vector2Int>(existing);
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var local in newLocal)
        {
            Vector2Int world = local + offset;
            foreach (var d in dirs)
            {
                if (existSet.Contains(world + d))
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    private int CountCorridorPairConnections(
        List<Vector2Int> newLocal,
        Vector2Int offset,
        List<Vector2Int> existing)
    {
        var existSet = new HashSet<Vector2Int>(existing);
        var newSet = new HashSet<Vector2Int>(newLocal);
        var checkedPairs = new HashSet<Vector2Int>();
        int pairCount = 0;

        foreach (var local in newLocal)
        {
            Vector2Int world = local + offset;
            Vector2Int[] checkDirs = { Vector2Int.up, Vector2Int.right };

            foreach (var d in checkDirs)
            {
                Vector2Int adjLocal = local + d;
                Vector2Int adjWorld = world + d;
                if (newSet.Contains(adjLocal)
                    && !checkedPairs.Contains(local)
                    && !checkedPairs.Contains(adjLocal))
                {
                    bool a = IsAdjacentToCorridor(world, existSet);
                    bool b = IsAdjacentToCorridor(adjWorld, existSet);
                    if (a && b)
                    {
                        pairCount++;
                        checkedPairs.Add(local);
                        checkedPairs.Add(adjLocal);
                    }
                }
            }
        }
        return pairCount;
    }

    private bool IsAdjacentToCorridor(Vector2Int pos, HashSet<Vector2Int> existSet)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
            if (existSet.Contains(pos + d))
                return true;
        return false;
    }

    private bool OverlapsExistingTiles(Tilemap source, Vector2Int offset, HashSet<Vector2Int> target)
    {
        foreach (var p in source.cellBounds.allPositionsWithin)
        {
            if (source.HasTile(p) && target.Contains((Vector2Int)p + offset))
                return true;
        }
        return false;
    }

    private void CopyTilemapWithTiles(
        Tilemap source,
        Vector2Int offset,
        HashSet<Vector2Int> targetSet,
        Dictionary<Vector2Int, TileBase> dict)
    {
        foreach (var p in source.cellBounds.allPositionsWithin)
        {
            if (!source.HasTile(p)) continue;
            Vector2Int world = (Vector2Int)p + offset;
            targetSet.Add(world);
            dict[world] = source.GetTile(p);
        }
    }

    private void GetTilemaps(
        GameObject roomPrefab,
        out Tilemap floorTM,
        out Tilemap wallTM,
        out Tilemap corridorTM)
    {
        var children = roomPrefab.GetComponentsInChildren<Transform>();
        Transform parent = children[1]; // 프로젝트 구조에 맞게 인덱스 조정
        floorTM = parent.Find("FloorTilemap").GetComponent<Tilemap>();
        wallTM = parent.Find("WallTilemap").GetComponent<Tilemap>();
        corridorTM = parent.Find("CorridorTilemap").GetComponent<Tilemap>();
    }
}

/// <summary>
/// List 셔플 확장 메서드
/// </summary>
public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
