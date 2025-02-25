using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MakeRandomMap : MonoBehaviour
{
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private int maxRooms = 5;
    [SerializeField] private SpreadTilemap spreadTilemap;
    [SerializeField] private GameObject player;

    // 랜덤 시드를 위한 변수: Inspector에서 직접 설정할 수도 있음
    [SerializeField] private int seed;
    // true이면 실행 시점의 시드(예: TickCount)를 사용하여 랜덤 초기화
    [SerializeField] private bool useRandomSeed = true;

    private HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();

    private void Start()
    {
        // 랜덤 시드 설정: useRandomSeed가 true라면 시스템 시드를 사용
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
        // 초기화
        spreadTilemap.ClearAllTiles();
        floorTiles.Clear();
        wallTiles.Clear();

        // 첫 방은 (0,0)에 생성
        Vector2Int startPos = Vector2Int.zero;
        // 시작 방은 항상 roomPrefabs[0] 사용 (원하는 경우 Random.Range로 변경 가능)
        PlaceRoom(roomPrefabs[0], startPos);

        // 이후 maxRooms-1 개의 방을 추가로 생성
        for (int i = 1; i < maxRooms; i++)
        {
            // 랜덤 프리팹을 골라서, 붙일 수 있는 위치를 찾는다
            GameObject nextRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            if (TryFindPlacementForRoom(nextRoomPrefab, out Vector2Int foundOffset))
            {
                // 겹치지 않는 위치를 찾았다면 방을 배치
                PlaceRoom(nextRoomPrefab, foundOffset);
            }
            else
            {
                // 더 이상 붙일 수 있는 곳이 없다면 중단
                break;
            }
        }

        // 타일맵 갱신
        spreadTilemap.SpreadFloorTilemap(floorTiles);
        spreadTilemap.SpreadWallTilemap(wallTiles);

        // 플레이어 위치 설정 (마지막에 생성된 방 위치로 할 수도 있고, 그냥 (0,0)으로 둘 수도 있음)
        player.transform.position = Vector2.zero;
    }

    /// <summary>
    /// 새 방(RoomPrefab)을 지정된 월드 오프셋(offset)에 실제로 배치(floorTiles, wallTiles에 복사)
    /// </summary>
    private void PlaceRoom(GameObject roomPrefab, Vector2Int offset)
    {
        // roomPrefab 내부의 FloorTilemap / WallTilemap 찾아서 각각 복사
        Tilemap floorTilemap, wallTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap);

        // 바닥 타일 복사
        CopyTilemap(floorTilemap, offset, floorTiles);
        // 벽 타일 복사
        CopyTilemap(wallTilemap, offset, wallTiles);
    }

    /// <summary>
    /// 새 방(RoomPrefab)을 벽과 벽이 정확히 1칸 차이로 맞닿게 배치할 수 있는 오프셋을 찾는다.
    /// 찾으면 true를 리턴하고, foundOffset에 그 오프셋을 담는다.
    /// </summary>
    private bool TryFindPlacementForRoom(GameObject roomPrefab, out Vector2Int foundOffset)
    {
        foundOffset = Vector2Int.zero;

        // 1) 새 프리팹의 WallTilemap(로컬 좌표들)을 구한다
        Tilemap floorTilemap, wallTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap);

        // 새 방의 벽 타일(로컬 좌표) 목록
        List<Vector2Int> newRoomLocalWallTiles = GetLocalWallPositions(wallTilemap);
        if (newRoomLocalWallTiles.Count == 0)
            return false;

        // 2) 기존 월드 상의 벽 타일 목록
        List<Vector2Int> existingWallList = new List<Vector2Int>(wallTiles);
        if (existingWallList.Count == 0)
        {
            // 아직 벽이 하나도 없는 상태라면, (0,0)에 그냥 붙인다고 가정
            // (보통 첫 방을 배치한 뒤라면 wallTiles가 있을 것이지만, 안전장치로 둠)
            existingWallList.Add(Vector2Int.zero);
        }

        existingWallList.Shuffle();
        newRoomLocalWallTiles.Shuffle();

        // 3) 기존 벽 타일 / 새 방 벽 타일 / 방향(상하좌우)을 조합하여
        //    "벽끼리 딱 붙도록" 오프셋을 계산하고, 배치 가능 여부(겹침 없음)를 확인
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int existingWall in existingWallList)
        {
            foreach (Vector2Int localWallPos in newRoomLocalWallTiles)
            {
                foreach (Vector2Int dir in directions)
                {
                    // 기존 벽(existingWall)과 새 벽(localWallPos)이 정확히 1칸 차이로 붙도록 오프셋 계산
                    Vector2Int offset = (existingWall + dir) - localWallPos;

                    // 이 offset으로 새 방을 배치했을 때, 기존 바닥/벽과 겹치지 않는지 검사
                    if (!OverlapsExistingTiles(floorTilemap, offset, floorTiles) &&
                        !OverlapsExistingTiles(wallTilemap, offset, wallTiles))
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
    /// tilemap 내의 모든 "벽 타일" (HasTile인 위치)를 로컬 좌표(Vector2Int)로 반환
    /// </summary>
    private List<Vector2Int> GetLocalWallPositions(Tilemap wallTilemap)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        BoundsInt bounds = wallTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos))
            {
                result.Add((Vector2Int)pos);
            }
        }
        return result;
    }

    /// <summary>
    /// 새 타일맵( floor or wall )을 offset만큼 평행이동했을 때,
    /// 기존의 targetSet( floorTiles or wallTiles )과 겹치는지 검사
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
                    // 이미 존재하는 타일과 겹침
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// tilemap의 모든 타일을 offset만큼 평행 이동하여 targetSet에 복사
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
    /// roomPrefab에서 FloorTilemap, WallTilemap을 찾아서 out 파라미터로 반환
    /// (각자의 프로젝트 구조에 맞게 수정 필요)
    /// </summary>
    private void GetTilemaps(GameObject roomPrefab, out Tilemap floorTilemap, out Tilemap wallTilemap)
    {
        // 예시: roomPrefab > (index 1) Child > FloorTilemap, WallTilemap
        Transform[] gridTransforms = roomPrefab.transform.GetComponentsInChildren<Transform>();

        // 실제 구조에 따라 달라질 수 있으니, 상황에 맞게 수정하세요.
        Transform tilemapParent = gridTransforms[1].transform;
        floorTilemap = tilemapParent.Find("FloorTilemap").GetComponent<Tilemap>();
        wallTilemap = tilemapParent.Find("WallTilemap").GetComponent<Tilemap>();
    }
}

/// <summary>
/// 리스트 셔플용 확장 메서드
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
