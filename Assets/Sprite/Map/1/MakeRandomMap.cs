using UnityEngine;
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

    private void Start()
    {
        if (useRandomSeed)
        {
            seed = System.Environment.TickCount;
        }
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

        // 첫 방은 (0,0)에 배치
        Vector2Int startPos = Vector2Int.zero;
        PlaceRoom(roomPrefabs[0], startPos);

        // 이후 maxRooms-1개의 방을 추가
        for (int i = 1; i < maxRooms; i++)
        {
            GameObject nextRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            if (TryFindPlacementForRoom(nextRoomPrefab, out Vector2Int foundOffset))
            {
                PlaceRoom(nextRoomPrefab, foundOffset);
            }
            else
            {
                // 더 이상 배치할 수 없다면 중단
                break;
            }
        }

        // 실제 타일맵에 Floor, Wall, Corridor 타일 반영
        spreadTilemap.SpreadFloorTilemap(floorTiles);
        spreadTilemap.SpreadWallTilemap(wallTiles);
        spreadTilemap.SpreadCorridorTilemap(corridorTiles);

        // 플레이어 위치 설정
        player.transform.position = Vector2.zero;
    }

    private void PlaceRoom(GameObject roomPrefab, Vector2Int offset)
    {
        Tilemap floorTilemap, wallTilemap, corridorTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap, out corridorTilemap);

        CopyTilemap(floorTilemap, offset, floorTiles);
        CopyTilemap(wallTilemap, offset, wallTiles);
        CopyTilemap(corridorTilemap, offset, corridorTiles);
    }

    /// <summary>
    /// 다수의 통로 타일이 정확히 연결되도록 수정된 로직.
    /// 새 방의 corridorTiles 중 최소 2개 이상이 기존 corridorTiles와 인접해야 배치가 허용됨.
    /// </summary>
    private bool TryFindPlacementForRoom(GameObject roomPrefab, out Vector2Int foundOffset)
    {
        foundOffset = Vector2Int.zero;

        // 새 방의 타일맵 가져오기
        Tilemap floorTilemap, wallTilemap, corridorTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap, out corridorTilemap);

        // 새 방의 corridor 로컬 좌표
        List<Vector2Int> newRoomLocalCorridorTiles = GetLocalCorridorPositions(corridorTilemap);
        if (newRoomLocalCorridorTiles.Count == 0)
            return false;

        // 기존 corridor 목록
        List<Vector2Int> existingCorridorList = new List<Vector2Int>(corridorTiles);
        if (existingCorridorList.Count == 0)
        {
            existingCorridorList.Add(Vector2Int.zero);
        }

        // 무작위 순회
        existingCorridorList.Shuffle();
        newRoomLocalCorridorTiles.Shuffle();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // 최소 연결해야 하는 통로 타일 수
        int requiredConnections = 2;

        foreach (Vector2Int existingCorridor in existingCorridorList)
        {
            foreach (Vector2Int localCorridorPos in newRoomLocalCorridorTiles)
            {
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int offset = (existingCorridor + dir) - localCorridorPos;

                    // Floor / Wall 겹침 검사
                    if (OverlapsExistingTiles(floorTilemap, offset, floorTiles)) continue;
                    if (OverlapsExistingTiles(wallTilemap, offset, wallTiles)) continue;

                    // 복수의 corridor 타일 연결 검사
                    int connectedCount = CountCorridorConnections(newRoomLocalCorridorTiles, offset, existingCorridorList);
                    if (connectedCount >= requiredConnections)
                    {
                        foundOffset = offset;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 새 방 corridorTiles를 offset만큼 이동했을 때,
    /// 기존 corridorTiles와 상하좌우 인접한 타일이 몇 개인지 계산
    /// </summary>
    private int CountCorridorConnections(
        List<Vector2Int> newRoomLocalCorridorTiles,
        Vector2Int offset,
        List<Vector2Int> existingCorridorList)
    {
        int count = 0;
        HashSet<Vector2Int> existingCorridorSet = new HashSet<Vector2Int>(existingCorridorList);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var localPos in newRoomLocalCorridorTiles)
        {
            Vector2Int worldPos = localPos + offset;
            foreach (var dir in directions)
            {
                Vector2Int neighbor = worldPos + dir;
                if (existingCorridorSet.Contains(neighbor))
                {
                    count++;
                    // 한 타일이 여러 방향에서 연결되더라도 한 번만 세고 넘어가려면 break
                    break;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// corridor 타일맵의 로컬 좌표들을 리스트로 반환
    /// </summary>
    private List<Vector2Int> GetLocalCorridorPositions(Tilemap corridorTilemap)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        BoundsInt bounds = corridorTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (corridorTilemap.HasTile(pos))
            {
                result.Add((Vector2Int)pos);
            }
        }
        return result;
    }

    private bool OverlapsExistingTiles(Tilemap sourceTilemap, Vector2Int offset, HashSet<Vector2Int> targetSet)
    {
        BoundsInt bounds = sourceTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (sourceTilemap.HasTile(pos))
            {
                Vector2Int adjustedPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);
                if (targetSet.Contains(adjustedPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void CopyTilemap(Tilemap sourceTilemap, Vector2Int offset, HashSet<Vector2Int> targetSet)
    {
        BoundsInt bounds = sourceTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (sourceTilemap.HasTile(pos))
            {
                Vector2Int adjustedPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);
                targetSet.Add(adjustedPos);
            }
        }
    }

    /// <summary>
    /// roomPrefab에서 FloorTilemap, WallTilemap, CorridorTilemap을 찾아서 out 파라미터로 반환
    /// </summary>
    private void GetTilemaps(GameObject roomPrefab, out Tilemap floorTilemap, out Tilemap wallTilemap, out Tilemap corridorTilemap)
    {
        Transform[] gridTransforms = roomPrefab.transform.GetComponentsInChildren<Transform>();
        // 이 부분은 프리팹 구조에 맞춰 수정 필요
        Transform tilemapParent = gridTransforms[1].transform;

        floorTilemap = tilemapParent.Find("FloorTilemap").GetComponent<Tilemap>();
        wallTilemap = tilemapParent.Find("WallTilemap").GetComponent<Tilemap>();
        corridorTilemap = tilemapParent.Find("CorridorTilemap").GetComponent<Tilemap>();
    }
}

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