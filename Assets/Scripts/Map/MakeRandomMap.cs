using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Collections;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class MakeRandomMap : MonoBehaviour
{
    [Header("=== 방 프리팹 및 플레이어 설정 ===")]
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private int maxRooms = 5;
    [SerializeField] private SpreadTilemap spreadTilemap;

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
    [SerializeField] private TileBase fillerBackgroundTile;

    //  방별 스폰 위치 저장
    private List<List<Vector2Int>> roomItemSpawnPositions = new List<List<Vector2Int>>();
    private List<RoomItemSettings> roomSettings = new List<RoomItemSettings>();
    private List<List<Vector2Int>> roomMonsterSpawnPositions = new List<List<Vector2Int>>();

    [Header("=== 광석 설정 ===")]
    [SerializeField][Range(0, 1f)] private float oreSpawnChance = 0.05f; // 타일당 등장 확률

    private HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> itemSpawnTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> monsterSpawnTiles = new HashSet<Vector2Int>();

    private Dictionary<Vector2Int, TileBase> floorTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> wallTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> corridorTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> itemSpawnTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> monsterSpawnTileDict = new Dictionary<Vector2Int, TileBase>();
    public Dictionary<Vector2Int, OreSetting> oreTileDict = new Dictionary<Vector2Int, OreSetting>();

    private List<BoundsInt> roomBounds = new List<BoundsInt>();
    
    public List<BoundsInt> RoomBounds => roomBounds;
    public List<List<Vector2Int>> RoomMonsterSpawnPositions => roomMonsterSpawnPositions;
    public List<RoomItemSettings> RoomSettings => roomSettings;
    public Vector2Int MapMin => mapMin;
    public Vector2Int MapMax => mapMax;

    private void Awake()
    {
        GameManager.instance.makeRandomMap = this;
    }

    public void GenerateMapFromSeed(int seed)
    {
        Random.InitState(seed);
        Debug.Log($"[MakeRandomMap] Generating map with seed: {seed}");

        // ★ roomMin/roomMax 자동 보정
        int RoomMinX = Mathf.Min(roomMin.x, roomMax.x);
        int RoomMaxX = Mathf.Max(roomMin.x, roomMax.x);
        int RoomMinY = Mathf.Min(roomMin.y, roomMax.y);
        int RoomMaxY = Mathf.Max(roomMin.y, roomMax.y);
        roomMin = new Vector2Int(RoomMinX, RoomMinY);
        roomMax = new Vector2Int(RoomMaxX, RoomMaxY);

        int mapMinX = Mathf.Min(mapMin.x, mapMax.x);
        int mapMaxX = Mathf.Max(mapMin.x, mapMax.x);
        int mapMinY = Mathf.Min(mapMin.y, mapMax.y);
        int mapMaxY = Mathf.Max(mapMin.y, mapMax.y);
        mapMin = new Vector2Int(mapMinX, mapMinY);
        mapMax = new Vector2Int(mapMaxX, mapMaxY);
        
        // 초기화
        roomBounds.Clear();
        roomItemSpawnPositions.Clear();
        roomSettings.Clear();
        roomMonsterSpawnPositions.Clear();

        spreadTilemap.ClearAllTiles();
        floorTiles.Clear(); floorTileDict.Clear();
        wallTiles.Clear(); wallTileDict.Clear();
        corridorTiles.Clear(); corridorTileDict.Clear();
        itemSpawnTiles.Clear(); itemSpawnTileDict.Clear();
        monsterSpawnTiles.Clear(); monsterSpawnTileDict.Clear();

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
        spreadTilemap.SpreadBackgroundTilemapWithTiles(floorTileDict);
        spreadTilemap.SpreadCorridorTilemapWithTiles(corridorTileDict);
        spreadTilemap.SpreadItemSpawnTilemapWithTiles(itemSpawnTileDict);
        spreadTilemap.SpreadWallTilemapWithTiles(wallTileDict);
        spreadTilemap.SpreadMonsterSpawnTilemapWithTiles(monsterSpawnTileDict);

        spreadTilemap.HideCorridorRenderer();
        spreadTilemap.HideItemSpawnRenderer();
        spreadTilemap.HideMonsterSpawnRenderer();

        spreadTilemap.FillGroundWithNoise(
            mapMin, mapMax,
            floorTiles, wallTiles, seed
        );

        GenerateOres();

        // 아이템 생성은 서버만 담당
        if (NetworkManager.Singleton.IsServer)
        {
            if (GameManager.instance.itemSpawner != null)
            {
                GameManager.instance.itemSpawner.SpawnItemsOnMap(roomItemSpawnPositions, roomSettings, spreadTilemap);
            }
            else
            {
                Debug.LogError("[MakeRandomMap] ItemSpawner가 할당되지 않았습니다!");
            }
        }
        
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkTransmission.instance.NotifyServerMapGeneratedServerRpc();
        }
    }

    private void GenerateOres()
    {
        // ItemDatabase에서 광석 목록을 불러옵니다.
        List<ItemData> availableOres = ItemDatabase.Instance.GetOreDataList();
        
        // 데이터베이스에 광석이 설정되어 있는지 확인
        if (availableOres == null || availableOres.Count == 0)
        {
            Debug.LogWarning("[MakeRandomMap] 데이터베이스에 생성할 광석이 없습니다. 광석 생성을 건너뜁니다.");
            return;
        }

        for (int x = mapMin.x; x <= mapMax.x; x++)
        {
            for (int y = mapMin.y; y <= mapMax.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // 방 바닥 또는 벽 위에는 스폰하지 않음
                if (floorTiles.Contains(pos) || wallTiles.Contains(pos))
                    continue;

                // oreSpawnChance 확률 체크
                if (Random.value > oreSpawnChance)
                    continue;

                // 실제 광석 배치
                // 데이터베이스에서 가져온 광석 중 하나를 무작위로 선택
                ItemData selectedOreData = availableOres[Random.Range(0, availableOres.Count)];
                
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                spreadTilemap.OreTilemap.SetTile(cellPos, selectedOreData.oreTile);

                // MapInteractionManager가 사용할 수 있도록 OreSetting 객체를 생성하여 딕셔너리에 저장
                // 드롭될 아이템의 가격은 해당 광석의 가격 범위 내에서 무작위로 결정
                int price = Random.Range(selectedOreData.priceRange.x, selectedOreData.priceRange.y + 1);
                OreSetting oreInfo = new OreSetting
                {
                    oreTile = selectedOreData.oreTile,
                    dropItem = new Inventory_Item(selectedOreData.itemId, price)
                };
                oreTileDict[pos] = oreInfo;
            }
        }
    }
    
    private void PlaceRoom(GameObject roomPrefab, Vector2Int offset)
    {
        GetTilemaps(roomPrefab,
                out Tilemap backgroundTM,
                out Tilemap wallTM,
                out Tilemap corridorTM,
                out Tilemap itemSpawnTM,
                out Tilemap monsterSpawnTM
        );
        CopyTilemapWithTiles(backgroundTM, offset, floorTiles, floorTileDict);
        CopyTilemapWithTiles(wallTM, offset, wallTiles, wallTileDict);
        CopyTilemapWithTiles(corridorTM, offset, corridorTiles, corridorTileDict);
        CopyTilemapWithTiles(itemSpawnTM, offset, itemSpawnTiles, itemSpawnTileDict);
        CopyTilemapWithTiles(monsterSpawnTM, offset, monsterSpawnTiles, monsterSpawnTileDict);

        var localBounds = backgroundTM.cellBounds;
        var worldOrigin = new Vector3Int(
            localBounds.xMin + offset.x,
            localBounds.yMin + offset.y,
            localBounds.zMin
        );
        roomBounds.Add(new BoundsInt(worldOrigin, localBounds.size));

        // 로컬→월드 좌표로 변환해 방별 리스트에 저장
        var localMonPos = GetLocalCorridorPositions(monsterSpawnTM);
        var worldMonPos = localMonPos
            .Select(p => p + offset)
            .ToList();
        roomMonsterSpawnPositions.Add(worldMonPos);

        var localPositions = GetLocalCorridorPositions(itemSpawnTM);
        var worldPositions = localPositions
            .Select(p => p + offset)
            .ToList();
        roomItemSpawnPositions.Add(worldPositions);

        var settings = roomPrefab.GetComponent<RoomItemSettings>();
        roomSettings.Add(settings);
    }

    private bool TryFindPlacementForRoom(
        GameObject roomPrefab,
        out Vector2Int foundOffset,
        out List<Vector2Int> connectionPoints)
    {
        foundOffset = Vector2Int.zero;
        connectionPoints = new List<Vector2Int>();

        GetTilemaps(roomPrefab, out Tilemap backgroundTM, out Tilemap wallTM, out Tilemap corridorTM, out Tilemap itemSpawnTM, out Tilemap monsterSpawnTM);
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
                    if (!IsWithinMapBounds(backgroundTM, offset) ||
                        !IsWithinMapBounds(wallTM, offset) ||
                        !IsWithinMapBounds(corridorTM, offset))
                        continue;

                    // 충돌 검사
                    if (OverlapsExistingTiles(backgroundTM, offset, floorTiles)) continue;
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

        if (fillerBackgroundTile != null)
        {
            floorTiles.Add(pos);
            floorTileDict[pos] = fillerBackgroundTile;
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

    private void GetTilemaps(GameObject roomPrefab, out Tilemap backgroundTM, out Tilemap wallTM, out Tilemap corridorTM, out Tilemap itemSpawnTM, out Tilemap monsterSpawnTM)
    {
        var children = roomPrefab.GetComponentsInChildren<Transform>();
        Transform parent = children[1];  // 프로젝트 구조에 따라 인덱스를 조정하세요
        backgroundTM = parent.Find("BackgroundTilemap").GetComponent<Tilemap>();
        wallTM = parent.Find("WallTilemap").GetComponent<Tilemap>();
        corridorTM = parent.Find("CorridorTilemap").GetComponent<Tilemap>();
        itemSpawnTM = parent.Find("ItemSpawnTilemap").GetComponent<Tilemap>();
        monsterSpawnTM = parent.Find("MonsterSpawnTilemap").GetComponent<Tilemap>();
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
        Gizmos.DrawWireCube(roomCenter, roomSize);
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
        Gizmos.DrawWireCube(mapCenter, mapSize);
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
