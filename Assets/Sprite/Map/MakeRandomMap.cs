using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;

public class MakeRandomMap : MonoBehaviour
{
    [Header("=== 방 프리팹 및 플레이어 설정 ===")]
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private int maxRooms = 5;
    [SerializeField] private SpreadTilemap spreadTilemap;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform player_SpawnPos;

    [Header("=== 시드 설정 ===")]
    [SerializeField] private int seed;
    [SerializeField] private bool useRandomSeed = true;

    [Header("=== 맵 생성 허용 범위 ===")]
    [Tooltip("맵 생성이 허용되는 최소 좌표 (x,y)")]
    [SerializeField] private Vector2Int mapMin;
    [Tooltip("맵 생성이 허용되는 최대 좌표 (x,y)")]
    [SerializeField] private Vector2Int mapMax;

    [Header("=== 벽 제거 시 채울 바닥 타일 ===")]
    [Tooltip("벽이 제거된 위치에 채울 바닥 타일을 할당하세요.")]
    [SerializeField] private TileBase fillerFloorTile;

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
        Debug.Log($"[MakeRandomMap] Using seed: {seed}");

        // ★ mapMin/mapMax 자동 보정
        int minX = Mathf.Min(mapMin.x, mapMax.x);
        int maxX = Mathf.Max(mapMin.x, mapMax.x);
        int minY = Mathf.Min(mapMin.y, mapMax.y);
        int maxY = Mathf.Max(mapMin.y, mapMax.y);
        mapMin = new Vector2Int(minX, minY);
        mapMax = new Vector2Int(maxX, maxY);

        GenerateMap();
    }


    public void GenerateMap()
    {
        // 초기화
        spreadTilemap.ClearAllTiles();
        floorTiles.Clear(); floorTileDict.Clear();
        wallTiles.Clear(); wallTileDict.Clear();
        corridorTiles.Clear(); corridorTileDict.Clear();

        // 첫 방 배치
        PlaceRoom(roomPrefabs[0], Vector2Int.zero);

        // 나머지 방 배치
        for (int i = 1; i < maxRooms; i++)
        {
            GameObject nextRoom = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            if (TryFindPlacementForRoom(nextRoom, out Vector2Int offset, out List<Vector2Int> connPoints))
            {
                PlaceRoom(nextRoom, offset);
                foreach (var pos in connPoints)
                    RemoveWallAndFillFloor(pos);
            }
            else
            {
                Debug.Log("[MakeRandomMap] 더 이상 배치 불가, 생성 종료");
                break;
            }
        }

        // 타일맵 반영
        spreadTilemap.SpreadFloorTilemapWithTiles(floorTileDict);
        spreadTilemap.SpreadCorridorTilemapWithTiles(corridorTileDict);
        spreadTilemap.SpreadWallTilemapWithTiles(wallTileDict);

        // 1) Corridor 숨기기
        spreadTilemap.HideCorridorRenderer();

        // 2) Ground 채우기 (방·벽 제외)
        spreadTilemap.FillGroundWithinBounds(
            mapMin, mapMax,
            floorTiles, wallTiles
        );

        // 3) 아이템 생성

        // 4) 몬스터 생성

        // 5) 플레이어 생성
        Instantiate_Player();

        // 6) 변수 세팅
        SettingManager();
    }

    private void Instantiate_Player()
    {
        GameObject player = Instantiate(playerPrefab, player_SpawnPos.position, Quaternion.identity);
        player.transform.position = player_SpawnPos.position;
    }

    private void SettingManager()
    {
        PlayerManager manager = PlayerManager.instance;
        manager.playerObject = GameObject.FindGameObjectWithTag("Player");
        manager.playerStats = manager.playerObject.GetComponent<Player_Stats>();
        manager.playerMove = manager.playerObject.GetComponent<Player_Move>();
        manager.cam = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CinemachineCamera>();
        manager.SettingCam();

        GameManager.instance.isSetting = true;

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
        var newCorridors = GetLocalCorridorPositions(corridorTM);
        if (newCorridors.Count == 0) return false;

        var existCorridors = new List<Vector2Int>(corridorTiles);
        if (existCorridors.Count == 0)
            existCorridors.Add(Vector2Int.zero);

        existCorridors.Shuffle();
        newCorridors.Shuffle();

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var exist in existCorridors)
        {
            foreach (var local in newCorridors)
            {
                foreach (var dir in dirs)
                {
                    var offset = (exist + dir) - local;

                    // 영역 제한
                    if (!IsWithinMapBounds(floorTM, offset) ||
                        !IsWithinMapBounds(wallTM, offset) ||
                        !IsWithinMapBounds(corridorTM, offset))
                        continue;

                    // 충돌 검사
                    if (OverlapsExistingTiles(floorTM, offset, floorTiles)) continue;
                    if (OverlapsExistingTiles(wallTM, offset, wallTiles)) continue;

                    // 연결 개수 검사
                    int req = (dir.y != 0) ? 1 : 2;
                    if (CountCorridorConnections(newCorridors, offset, existCorridors) < req) continue;
                    if (dir.y == 0 &&
                        CountCorridorPairConnections(newCorridors, offset, existCorridors) == 0)
                        continue;

                    // 연결 지점 찾기
                    var points = FindConnectionPoints(newCorridors, offset, existCorridors);
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

    /// <summary>
    /// 새 방 통로(newLocal)와 기존 통로(existing)의 인접점을 찾아,
    /// 제거할 벽 좌표(양쪽) 리스트로 반환
    /// </summary>
    private List<Vector2Int> FindConnectionPoints(
        List<Vector2Int> newLocal,
        Vector2Int offset,
        List<Vector2Int> existing)
    {
        var result = new List<Vector2Int>();
        var existSet = new HashSet<Vector2Int>(existing);
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var local in newLocal)
        {
            Vector2Int world = local + offset;
            foreach (var d in dirs)
            {
                Vector2Int neighbor = world + d;
                if (existSet.Contains(neighbor))
                {
                    // 벽은 양쪽 중간 위치
                    Vector2Int wall1 = world + (d / 2);
                    Vector2Int wall2 = neighbor - (d / 2);
                    result.Add(wall1);
                    result.Add(wall2);
                }
            }
        }
        return result;
    }

    private void RemoveWallAndFillFloor(Vector2Int pos)
    {
        if (wallTiles.Remove(pos))
            wallTileDict.Remove(pos);

        if (fillerFloorTile != null)
        {
            floorTiles.Add(pos);
            floorTileDict[pos] = fillerFloorTile;
        }
    }

    private bool IsWithinMapBounds(Tilemap source, Vector2Int offset)
    {
        foreach (var p in source.cellBounds.allPositionsWithin)
        {
            if (!source.HasTile(p)) continue;
            var wp = (Vector2Int)p + offset;
            if (wp.x < mapMin.x || wp.x > mapMax.x || wp.y < mapMin.y || wp.y > mapMax.y)
                return false;
        }
        return true;
    }

    private List<Vector2Int> GetLocalCorridorPositions(Tilemap tm)
    {
        var list = new List<Vector2Int>();
        foreach (var p in tm.cellBounds.allPositionsWithin)
            if (tm.HasTile(p))
                list.Add((Vector2Int)p);
        return list;
    }

    private int CountCorridorConnections(List<Vector2Int> newLocal, Vector2Int offset, List<Vector2Int> existing)
    {
        int count = 0;
        var existSet = new HashSet<Vector2Int>(existing);
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var local in newLocal)
        {
            var world = local + offset;
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

    private int CountCorridorPairConnections(List<Vector2Int> newLocal, Vector2Int offset, List<Vector2Int> existing)
    {
        int pairCount = 0;
        var existSet = new HashSet<Vector2Int>(existing);
        var newSet = new HashSet<Vector2Int>(newLocal);
        var checkedSet = new HashSet<Vector2Int>();

        foreach (var local in newLocal)
        {
            var world = local + offset;
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right };

            foreach (var d in dirs)
            {
                var adjLocal = local + d;
                var adjWorld = world + d;
                if (!newSet.Contains(adjLocal)) continue;
                if (checkedSet.Contains(local) || checkedSet.Contains(adjLocal)) continue;

                if (existSet.Contains(world + Vector2Int.up) || existSet.Contains(world + Vector2Int.down) ||
                    existSet.Contains(world + Vector2Int.left) || existSet.Contains(world + Vector2Int.right))
                {
                    if (existSet.Contains(adjWorld + Vector2Int.up) || existSet.Contains(adjWorld + Vector2Int.down) ||
                        existSet.Contains(adjWorld + Vector2Int.left) || existSet.Contains(adjWorld + Vector2Int.right))
                    {
                        pairCount++;
                        checkedSet.Add(local);
                        checkedSet.Add(adjLocal);
                    }
                }
            }
        }
        return pairCount;
    }

    private bool OverlapsExistingTiles(Tilemap source, Vector2Int offset, HashSet<Vector2Int> target)
    {
        foreach (var p in source.cellBounds.allPositionsWithin)
        {
            if (!source.HasTile(p)) continue;
            if (target.Contains((Vector2Int)p + offset))
                return true;
        }
        return false;
    }

    private void CopyTilemapWithTiles(Tilemap source, Vector2Int offset, HashSet<Vector2Int> targetSet, Dictionary<Vector2Int, TileBase> dict)
    {
        foreach (var p in source.cellBounds.allPositionsWithin)
        {
            if (!source.HasTile(p)) continue;
            var world = (Vector2Int)p + offset;
            targetSet.Add(world);
            dict[world] = source.GetTile(p);
        }
    }

    private void GetTilemaps(GameObject roomPrefab, out Tilemap floorTM, out Tilemap wallTM, out Tilemap corridorTM)
    {
        var children = roomPrefab.GetComponentsInChildren<Transform>();
        Transform parent = children[1];  // 프로젝트 구조에 따라 인덱스를 조정하세요
        floorTM = parent.Find("FloorTilemap").GetComponent<Tilemap>();
        wallTM = parent.Find("WallTilemap").GetComponent<Tilemap>();
        corridorTM = parent.Find("CorridorTilemap").GetComponent<Tilemap>();
    }

    // 맵 생성 허용 범위를 Gizmos로 시각화
    private void OnDrawGizmos()
    {
        // 에디터에서만 실행
        #if UNITY_EDITOR
        // 범위의 가운데와 크기 계산
        Vector3 center = new Vector3(
            (mapMin.x + mapMax.x + 1) * 0.5f,
            (mapMin.y + mapMax.y + 1) * 0.5f,
            0f
        );
        Vector3 size = new Vector3(
            Mathf.Abs(mapMax.x - mapMin.x + 1),
            Mathf.Abs(mapMax.y - mapMin.y + 1),
            0f
        );

        // 선으로만 그리기
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);  // wireframe box :contentReference[oaicite:0]{index=0}
        #endif
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
