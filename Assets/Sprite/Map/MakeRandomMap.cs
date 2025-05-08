using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;
using System.Linq;

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

    [Header("=== 방 생성 허용 범위 ===")]
    [Tooltip("방 생성이 허용되는 최소 좌표 (x,y)")]
    [SerializeField] private Vector2Int roomMin;
    [Tooltip("방 생성이 허용되는 최대 좌표 (x,y)")]
    [SerializeField] private Vector2Int roomMax;

    [Header("=== 맵 생성 허용 범위 ===")]
    [Tooltip("맵 생성이 허용되는 최소 좌표 (x,y)")]
    [SerializeField] private Vector2Int mapMin;
    [Tooltip("맵 생성이 허용되는 최대 좌표 (x,y)")]
    [SerializeField] private Vector2Int mapMax;

    [Header("=== 벽 제거 시 채울 바닥 타일 ===")]
    [Tooltip("벽이 제거된 위치에 채울 바닥 타일을 할당하세요.")]
    [SerializeField] private TileBase fillerFloorTile;

    private List<List<Vector2Int>> roomItemSpawnPositions = new List<List<Vector2Int>>();
    [Header("=== 아이템 드롭 설정 ===")]
    [Tooltip("드롭할 프리팹을 할당하세요.")]
    [SerializeField] private GameObject dropPrefab;

    [Tooltip("드롭 가능한 아이템 데이터 리스트")]
    [SerializeField] private List<ItemData> itemList;

    private HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> itemSpawnTiles = new HashSet<Vector2Int>();

    private Dictionary<Vector2Int, TileBase> floorTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> wallTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> corridorTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> itemSpawnTileDict = new Dictionary<Vector2Int, TileBase>();

    private void Start()
    {
        if (useRandomSeed)
            seed = System.Environment.TickCount;
        Random.InitState(seed);
        Debug.Log($"[MakeRandomMap] Using seed: {seed}");

        // ★ roomMin/roomMax 자동 보정
        int RoomMinX = Mathf.Min(roomMin.x, roomMax.x);
        int RoomMaxX = Mathf.Max(roomMin.x, roomMax.x);
        int RoomMinY = Mathf.Min(roomMin.y, roomMax.y);
        int RoomMaxY = Mathf.Max(roomMin.y, roomMax.y);
        roomMin = new Vector2Int(RoomMinX, RoomMinY);
        roomMax = new Vector2Int(RoomMaxX, RoomMaxY);

        // ★ roomMin/roomMax 자동 보정
        int mapMinX = Mathf.Min(roomMin.x, roomMax.x);
        int mapMaxX = Mathf.Max(roomMin.x, roomMax.x);
        int mapMinY = Mathf.Min(roomMin.y, roomMax.y);
        int mapMaxY = Mathf.Max(roomMin.y, roomMax.y);
        roomMin = new Vector2Int(mapMinX, mapMinY);
        roomMax = new Vector2Int(mapMaxX, mapMaxY);

        GenerateMap();
    }


    public void GenerateMap()
    {
        // 초기화
        spreadTilemap.ClearAllTiles();
        floorTiles.Clear(); floorTileDict.Clear();
        wallTiles.Clear(); wallTileDict.Clear();
        corridorTiles.Clear(); corridorTileDict.Clear();
        itemSpawnTiles.Clear(); itemSpawnTileDict.Clear();

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
        spreadTilemap.SpreadItemSpawnTilemapWithTiles(itemSpawnTileDict);
        spreadTilemap.SpreadWallTilemapWithTiles(wallTileDict);

        // 1) Corridor 숨기기
        spreadTilemap.HideCorridorRenderer();
        spreadTilemap.HideItemSpawnRenderer();

        // 2) Ground 채우기 (방·벽 제외)
        spreadTilemap.FillGroundWithNoise(
            mapMin, mapMax,
            floorTiles, wallTiles, seed
        );

        // 3) 아이템 생성
        DropItems();


        // 4) 몬스터 생성

        // 5) 플레이어 생성
        Instantiate_Player();

        // 6) 변수 세팅
        SettingManager();
    }

    /// <summary>
    /// 각 방마다 2~3개의 아이템을 랜덤 드롭합니다.
    /// </summary>
    private void DropItems()
    {
        if (itemList == null || itemList.Count == 0) return;

        for (int i = 0; i < roomItemSpawnPositions.Count; i++)
        {
            var spawnPositions = roomItemSpawnPositions[i];
            if (spawnPositions.Count == 0) continue;

            // 방당 드롭 개수: 2~3개 (maxExclusive 뺌) :contentReference[oaicite:0]{index=0}
            int dropCount = Random.Range(2, 4);

            for (int j = 0; j < dropCount; j++)
            {
                // 랜덤 셀 선택
                int idx = Random.Range(0, spawnPositions.Count);
                Vector3Int cellPos = (Vector3Int)spawnPositions[idx];
                Vector3 worldPos = spreadTilemap.ItemSpawnTilemap
                    .CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0f);

                // 드롭 프리팹 인스턴스화 :contentReference[oaicite:1]{index=1}
                GameObject drop = Instantiate(dropPrefab, worldPos, Quaternion.identity);

                // 랜덤 아이템 데이터 & 속도 설정
                ItemData data = itemList[Random.Range(0, itemList.Count)];
                Vector2 velocity = new Vector2(
                    Random.Range(-5f, 5f),    // X 속도 :contentReference[oaicite:2]{index=2}
                    Random.Range(15f, 20f)    // Y 속도 :contentReference[oaicite:3]{index=3}
                );

                drop.GetComponent<ItemObject>().SetupItem(data, velocity);
            }
        }
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
        GetTilemaps(roomPrefab,
                out Tilemap floorTM,
                out Tilemap wallTM,
                out Tilemap corridorTM,
                out Tilemap itemSpawnTM);
        CopyTilemapWithTiles(floorTM, offset, floorTiles, floorTileDict);
        CopyTilemapWithTiles(wallTM, offset, wallTiles, wallTileDict);
        CopyTilemapWithTiles(corridorTM, offset, corridorTiles, corridorTileDict);
        CopyTilemapWithTiles(itemSpawnTM, offset, itemSpawnTiles, itemSpawnTileDict);

        var localPositions = GetLocalCorridorPositions(itemSpawnTM);
        var worldPositions = localPositions
            .Select(p => p + offset)
            .ToList();
        roomItemSpawnPositions.Add(worldPositions);
    }

    private bool TryFindPlacementForRoom(
        GameObject roomPrefab,
        out Vector2Int foundOffset,
        out List<Vector2Int> connectionPoints)
    {
        foundOffset = Vector2Int.zero;
        connectionPoints = new List<Vector2Int>();

        GetTilemaps(roomPrefab, out Tilemap floorTM, out Tilemap wallTM, out Tilemap corridorTM, out Tilemap itemSpawnTM);
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
            if (wp.x < roomMin.x || wp.x > roomMax.x || wp.y < roomMin.y || wp.y > roomMax.y)
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

    private void GetTilemaps(GameObject roomPrefab, out Tilemap floorTM, out Tilemap wallTM, out Tilemap corridorTM, out Tilemap itemSpawnTM)
    {
        var children = roomPrefab.GetComponentsInChildren<Transform>();
        Transform parent = children[1];  // 프로젝트 구조에 따라 인덱스를 조정하세요
        floorTM = parent.Find("FloorTilemap").GetComponent<Tilemap>();
        wallTM = parent.Find("WallTilemap").GetComponent<Tilemap>();
        corridorTM = parent.Find("CorridorTilemap").GetComponent<Tilemap>();
        itemSpawnTM = parent.Find("ItemSpawnTilemap").GetComponent<Tilemap>();
    }

    // 맵 생성 허용 범위를 Gizmos로 시각화
    private void OnDrawGizmos()
    {
        // 에디터에서만 실행
        #if UNITY_EDITOR
        // 범위의 가운데와 크기 계산
        Vector3 roomCenter = new Vector3(
            (roomMin.x + roomMax.x + 1) * 0.5f,
            (roomMin.y + roomMax.y + 1) * 0.5f,
            0f
        );
        Vector3 roomSize = new Vector3(
            Mathf.Abs(roomMax.x - roomMin.x + 1),
            Mathf.Abs(roomMax.y - roomMin.y + 1),
            0f
        );

        // 선으로만 그리기
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(roomCenter, roomSize);  // wireframe box :contentReference[oaicite:0]{index=0}
        #endif

        // 에디터에서만 실행
        #if UNITY_EDITOR
        // 범위의 가운데와 크기 계산
        Vector3 mapCenter = new Vector3(
            (mapMin.x + mapMax.x + 1) * 0.5f,
            (mapMin.y + mapMax.y + 1) * 0.5f,
            0f
        );
        Vector3 mapSize = new Vector3(
            Mathf.Abs(mapMax.x - mapMin.x + 1),
            Mathf.Abs(mapMax.y - mapMin.y + 1),
            0f
        );

        // 선으로만 그리기
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(mapCenter, mapSize);  // wireframe box :contentReference[oaicite:0]{index=0}
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
