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

        // 이후 maxRooms-1개의 방 생성
        for (int i = 1; i < maxRooms; i++)
        {
            GameObject nextRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            if (TryFindPlacementForRoom(nextRoomPrefab, out Vector2Int foundOffset))
            {
                PlaceRoom(nextRoomPrefab, foundOffset);
            }
            else
            {
                // 더 이상 배치 불가하면 중단
                break;
            }
        }

        // 실제 타일맵에 Floor, Wall, Corridor 배치
        spreadTilemap.SpreadFloorTilemap(floorTiles);
        spreadTilemap.SpreadWallTilemap(wallTiles);
        spreadTilemap.SpreadCorridorTilemap(corridorTiles);

        // 플레이어 위치 설정
        player.transform.position = Vector2.zero;
    }

    /// <summary>
    /// 방(프리팹) 타일맵을 오프셋 위치에 복사
    /// </summary>
    private void PlaceRoom(GameObject roomPrefab, Vector2Int offset)
    {
        Tilemap floorTilemap, wallTilemap, corridorTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap, out corridorTilemap);

        CopyTilemap(floorTilemap, offset, floorTiles);
        CopyTilemap(wallTilemap, offset, wallTiles);
        CopyTilemap(corridorTilemap, offset, corridorTiles);
    }

    /// <summary>
    /// 새 방을 기존 맵에 붙일 수 있는 오프셋을 찾는 과정:
    ///  - 세로 방향(up/down)은 통로 1개 이상 연결이면 OK
    ///  - 가로 방향(left/right)은 통로 2개 이상 연결이어야 OK
    ///  - 연결이 성사되면 연결 지점의 벽 타일을 제거
    /// </summary>
    private bool TryFindPlacementForRoom(GameObject roomPrefab, out Vector2Int foundOffset)
    {
        foundOffset = Vector2Int.zero;

        // 새 방(프리팹)의 Tilemap들 가져오기
        Tilemap floorTilemap, wallTilemap, corridorTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap, out corridorTilemap);

        // 새 방 통로 타일 (로컬 좌표)
        List<Vector2Int> newRoomLocalCorridorTiles = GetLocalCorridorPositions(corridorTilemap);
        if (newRoomLocalCorridorTiles.Count == 0)
            return false;

        // 기존 통로 타일 목록
        List<Vector2Int> existingCorridorList = new List<Vector2Int>(corridorTiles);
        if (existingCorridorList.Count == 0)
        {
            // 아직 통로가 없다면 (0,0) 가상 통로라고 가정
            existingCorridorList.Add(Vector2Int.zero);
        }

        // 무작위 순회(셔플)
        existingCorridorList.Shuffle();
        newRoomLocalCorridorTiles.Shuffle();

        // 상하좌우 방향
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int existingCorridor in existingCorridorList)
        {
            foreach (Vector2Int localCorridorPos in newRoomLocalCorridorTiles)
            {
                foreach (Vector2Int dir in directions)
                {
                    // 세로: 1개 연결, 가로: 2개 연결
                    int requiredConnections = (dir.y != 0) ? 1 : 2;

                    // 배치 오프셋 계산
                    Vector2Int offset = (existingCorridor + dir) - localCorridorPos;

                    // Floor/Wall 겹침 검사
                    if (OverlapsExistingTiles(floorTilemap, offset, floorTiles)) continue;
                    if (OverlapsExistingTiles(wallTilemap, offset, wallTiles)) continue;

                    // 통로 연결 검사
                    int connectedCount = CountCorridorConnections(newRoomLocalCorridorTiles, offset, existingCorridorList);
                    if (connectedCount >= requiredConnections)
                    {
                        // 연결 성사!
                        foundOffset = offset;

                        // 연결 지점의 벽 타일 제거하기
                        // -> 기존 통로 타일(existingCorridor)와 새 통로 타일이 붙는 지점 = (existingCorridor + dir)
                        //    만약 그 자리에 벽 타일이 있었다면 없앤다.
                        Vector2Int connectingPos = existingCorridor + dir;
                        if (wallTiles.Contains(connectingPos))
                        {
                            wallTiles.Remove(connectingPos);
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 새 방 corridor 타일들을 offset만큼 옮겼을 때,
    /// 기존 corridor 타일들과 상하좌우로 인접한(= 연결된) 타일 개수를 센다.
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
                    // 한 타일이 여러 방향으로 연결되어도 '1번'으로만 세려면 break
                    break;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// corridorTilemap 내의 모든 타일 좌표를 리스트로 반환
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

    /// <summary>
    /// sourceTilemap을 offset만큼 옮겼을 때, targetSet과 겹치는지 검사
    /// </summary>
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

    /// <summary>
    /// sourceTilemap 내 모든 타일을 offset만큼 평행 이동하여 targetSet에 복사
    /// </summary>
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
    /// roomPrefab에서 FloorTilemap, WallTilemap, CorridorTilemap 찾기
    /// (프로젝트 구조에 맞춰 수정 필요)
    /// </summary>
    private void GetTilemaps(GameObject roomPrefab, out Tilemap floorTilemap, out Tilemap wallTilemap, out Tilemap corridorTilemap)
    {
        Transform[] gridTransforms = roomPrefab.transform.GetComponentsInChildren<Transform>();
        // 예시로 1번 인덱스 자식이 타일맵 부모라고 가정
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