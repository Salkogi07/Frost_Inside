using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class VisualMakeRandomMap : MonoBehaviour
{
    [Header("=== 방 프리팹 및 플레이어 설정 ===")]
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private int maxRooms = 5;
    public SpreadTilemap spreadTilemap;

    [Header("=== 시드 설정 ===")]
    [SerializeField] private int seed;
    [SerializeField] private bool useRandomSeed = true;
    
    [Header("=== 시각적 생성 딜레이 (초) ===")]
    [SerializeField] private float placementDelay = 0.2f; // 방 배치 딜레이
    [SerializeField] private float connectionDelay = 0.1f; // 통로 연결 딜레이
    [SerializeField] private float postProcessDelay = 0.5f; // 후처리 (바닥, 광석) 딜레이

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
    [SerializeField][Range(0, 1f)] private float oreSpawnChance = 0.005f;

    public GameObject crewObject;

    private HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> propsTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> itemSpawnTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> monsterSpawnTiles = new HashSet<Vector2Int>();

    private Dictionary<Vector2Int, TileBase> floorTileDict = new Dictionary<Vector2Int, TileBase>();
    private Dictionary<Vector2Int, TileBase> propsTileDict = new Dictionary<Vector2Int, TileBase>();
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

    private void Start()
    {
        int seed = System.Environment.TickCount;
        StartVisualMapGeneration(seed);
    }

    /// <summary>
    /// 외부에서 맵 생성을 시작하기 위한 메서드입니다.
    /// </summary>
    public void StartVisualMapGeneration(int newSeed)
    {
        StartCoroutine(GenerateMapVisual(newSeed));
    }

    private IEnumerator GenerateMapVisual(int generationSeed)
    {
        Random.InitState(generationSeed);
        Debug.Log($"[VisualMakeRandomMap] Generating map with seed: {generationSeed}");
        
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
        propsTiles.Clear(); propsTileDict.Clear();

        // 첫 방 배치
        PlaceRoom(roomPrefabs[0], Vector2Int.zero);
        UpdateTilemaps();
        yield return new WaitForSeconds(placementDelay);

        // 나머지 방 배치
        for (int i = 1; i < maxRooms; i++)
        {
            GameObject nextRoom = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            if (TryFindPlacementForRoom(nextRoom, out Vector2Int offset, out List<Vector2Int> connPoints))
            {
                PlaceRoom(nextRoom, offset);
                UpdateTilemaps();
                yield return new WaitForSeconds(placementDelay);

                foreach (var pos in connPoints)
                {
                    RemoveWallAndFillFloor(pos);
                }
                UpdateTilemaps(); // 연결부 갱신
                yield return new WaitForSeconds(connectionDelay);
            }
            else
            {
                Debug.Log("[VisualMakeRandomMap] 더 이상 배치 불가, 생성 종료");
                break;
            }
        }

        // 숨겨야 할 타일맵 렌더러 숨기기
        spreadTilemap.HideCorridorRenderer();
        spreadTilemap.HideItemSpawnRenderer();
        spreadTilemap.HideMonsterSpawnRenderer();

        // 주변 지형 채우기
        yield return new WaitForSeconds(postProcessDelay);
        spreadTilemap.FillGroundWithNoise(
            mapMin, mapMax,
            floorTiles, wallTiles, generationSeed
        );

        // 광석 생성
        yield return new WaitForSeconds(postProcessDelay);
        GenerateOres();
        
        crewObject.SetActive(true);
    }

    /// <summary>
    /// 현재까지의 타일 데이터를 타일맵에 즉시 반영합니다.
    /// </summary>
    private void UpdateTilemaps()
    {
        spreadTilemap.SpreadBackgroundTilemapWithTiles(floorTileDict);
        spreadTilemap.SpreadPropsTilemapWithTiles(propsTileDict);
        spreadTilemap.SpreadCorridorTilemapWithTiles(corridorTileDict);
        spreadTilemap.SpreadItemSpawnTilemapWithTiles(itemSpawnTileDict);
        spreadTilemap.SpreadWallTilemapWithTiles(wallTileDict);
        spreadTilemap.SpreadMonsterSpawnTilemapWithTiles(monsterSpawnTileDict);
    }
    
    private void GenerateOres()
    {
        List<ItemData> availableOres = ItemDatabase.Instance.GetOreDataList();
        
        if (availableOres == null || availableOres.Count == 0)
        {
            Debug.LogWarning("[VisualMakeRandomMap] 데이터베이스에 생성할 광석이 없습니다. 광석 생성을 건너뜁니다.");
            return;
        }

        for (int x = mapMin.x; x <= mapMax.x; x++)
        {
            for (int y = mapMin.y; y <= mapMax.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (floorTiles.Contains(pos) || wallTiles.Contains(pos))
                    continue;

                if (Random.value > oreSpawnChance)
                    continue;

                ItemData selectedOreData = availableOres[Random.Range(0, availableOres.Count)];
                
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                spreadTilemap.OreTilemap.SetTile(cellPos, selectedOreData.oreTile);

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
                out Tilemap monsterSpawnTM,
                out Tilemap propsTM
        );
        CopyTilemapWithTiles(backgroundTM, offset, floorTiles, floorTileDict);
        CopyTilemapWithTiles(wallTM, offset, wallTiles, wallTileDict);
        CopyTilemapWithTiles(corridorTM, offset, corridorTiles, corridorTileDict);
        CopyTilemapWithTiles(itemSpawnTM, offset, itemSpawnTiles, itemSpawnTileDict);
        CopyTilemapWithTiles(monsterSpawnTM, offset, monsterSpawnTiles, monsterSpawnTileDict);
        CopyTilemapWithTiles(propsTM, offset, propsTiles, propsTileDict);

        var localBounds = backgroundTM.cellBounds;
        var worldOrigin = new Vector3Int(
            localBounds.xMin + offset.x,
            localBounds.yMin + offset.y,
            localBounds.zMin
        );
        roomBounds.Add(new BoundsInt(worldOrigin, localBounds.size));

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

        GetTilemaps(roomPrefab, out Tilemap backgroundTM, out Tilemap wallTM, out Tilemap corridorTM, out Tilemap itemSpawnTM, out Tilemap monsterSpawnTM, out Tilemap propsTM);
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

                    if (!IsWithinMapBounds(backgroundTM, offset) ||
                        !IsWithinMapBounds(wallTM, offset) ||
                        !IsWithinMapBounds(corridorTM, offset))
                        continue;

                    if (OverlapsExistingTiles(backgroundTM, offset, floorTiles)) continue;
                    if (OverlapsExistingTiles(wallTM, offset, wallTiles)) continue;
                    
                    int req = (dir.y != 0) ? 1 : 2;
                    if (CountCorridorConnections(newCorridors, offset, existCorridors) < req) continue;
                    if (dir.y == 0 &&
                        CountCorridorPairConnections(newCorridors, offset, existCorridors) == 0)
                        continue;

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

    private void GetTilemaps(GameObject roomPrefab, out Tilemap backgroundTM, out Tilemap wallTM, out Tilemap corridorTM, out Tilemap itemSpawnTM, out Tilemap monsterSpawnTM, out Tilemap propsTM)
    {
        var children = roomPrefab.GetComponentsInChildren<Transform>();
        Transform parent = children[1];
        backgroundTM = parent.Find("BackgroundTilemap").GetComponent<Tilemap>();
        wallTM = parent.Find("WallTilemap").GetComponent<Tilemap>();
        corridorTM = parent.Find("CorridorTilemap").GetComponent<Tilemap>();
        itemSpawnTM = parent.Find("ItemSpawnTilemap").GetComponent<Tilemap>();
        monsterSpawnTM = parent.Find("MonsterSpawnTilemap").GetComponent<Tilemap>();
        propsTM = parent.Find("PropsTilemap").GetComponent<Tilemap>();
    }
    
    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
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

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(roomCenter, roomSize);
        
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
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(mapCenter, mapSize);
        #endif
    }
}