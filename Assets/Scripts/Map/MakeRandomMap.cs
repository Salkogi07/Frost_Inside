using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;
using System.Linq;
using System.Collections;
using Map;
using Random = UnityEngine.Random;

public class MakeRandomMap : MonoBehaviour
{
    private PlayerManager _playerManager;
    private CinemachineCamera _camera;
    
    [Header("=== 방 프리팹 및 플레이어 설정 ===")]
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private int maxRooms = 5;
    [SerializeField] private SpreadTilemap spreadTilemap;
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

    // ① 방별 스폰 위치 저장
    private List<List<Vector2Int>> roomItemSpawnPositions = new List<List<Vector2Int>>();
    private List<RoomItemSettings> roomSettings = new List<RoomItemSettings>();
    private List<List<Vector2Int>> roomMonsterSpawnPositions = new List<List<Vector2Int>>();

    [Header("=== 아이템 부모 설정===")]
    [SerializeField] private Transform dropParent;

    [Header("=== 몬스터 부모 설정 ===")]
    [SerializeField] private Transform monsterParent;

    [Header("몬스터 스폰 간격 설정")]
    [Tooltip("다음 스폰까지 최소 대기 시간(초)")]
    [SerializeField] private float minSpawnInterval = 10f;
    [Tooltip("다음 스폰까지 최대 대기 시간(초)")]
    [SerializeField] private float maxSpawnInterval = 30f;

    [Header("몬스터 스폰 개수 설정")]
    [Tooltip("한 사이클당 최소 소환할 몬스터 수")]
    [SerializeField] private int minSpawnCount = 1;
    [Tooltip("한 사이클당 최대 소환할 몬스터 수")]
    [SerializeField] private int maxSpawnCount = 3;

    [Header("=== 아이템 드롭 설정 ===")]
    private const int minTotalPriceSum = 1800;
    private const int maxTotalPriceSum = 2200;

    [Tooltip("드롭할 프리팹을 할당하세요.")]
    [SerializeField] private GameObject dropPrefab;

    [Tooltip("드롭 가능한 아이템 데이터 리스트")]
    [SerializeField] private List<ItemData> itemList;

    [Header("=== 광석 설정 ===")]
    [SerializeField] private List<OreSetting> oreSettings;     // Inspector에서 여러 종류 설정
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

    private void Awake()
    {
        _playerManager = GetComponent<PlayerManager>();
    }

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
        spreadTilemap.SpreadFloorTilemapWithTiles(floorTileDict);
        spreadTilemap.SpreadCorridorTilemapWithTiles(corridorTileDict);
        spreadTilemap.SpreadItemSpawnTilemapWithTiles(itemSpawnTileDict);
        spreadTilemap.SpreadWallTilemapWithTiles(wallTileDict);
        spreadTilemap.SpreadMonsterSpawnTilemapWithTiles(monsterSpawnTileDict);

        // 1) Corridor 숨기기
        spreadTilemap.HideCorridorRenderer();
        spreadTilemap.HideItemSpawnRenderer();
        spreadTilemap.HideMonsterSpawnRenderer();

        // 2) Ground 채우기 (방·벽 제외)
        spreadTilemap.FillGroundWithNoise(
            mapMin, mapMax,
            floorTiles, wallTiles, seed
        );

        // 2.5) 광석 생성
        GenerateOres();

        // 3) 아이템 생성
        DropItems();


        // 4) 몬스터 생성
        StartCoroutine(SpawnMonstersRoutine());

        // 5) 플레이어 생성
        Instantiate_Player();

        // 6) 변수 세팅
        SettingManager();
    }

    private void GenerateOres()
    {
        for (int x = mapMin.x; x <= mapMax.x; x++)
        {
            for (int y = mapMin.y; y <= mapMax.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // ① 방 바닥 또는 벽 위에는 스폰하지 않음
                if (floorTiles.Contains(pos) || wallTiles.Contains(pos))
                    continue;

                // ② oreSpawnChance 확률 체크
                if (Random.value > oreSpawnChance)
                    continue;

                // ③ 실제 광석 배치
                var ore = oreSettings[Random.Range(0, oreSettings.Count)];
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                spreadTilemap.OreTilemap.SetTile(cellPos, ore.oreTile);  // Tilemap.SetTile API :contentReference[oaicite:0]{index=0}
                oreTileDict[pos] = ore;
            }
        }
    }


    private void DropItems()
    {
        // 1) 드롭 정보 수집용 리스트와 합산 변수
        var dropInfos = new List<(ItemData data, Vector3 pos, Vector2 vel, int price)>();
        int totalPriceSum = 0;
        bool reachedMax = false;  // maxTotalPriceSum 초과 플래그

        // 방별 스폰 위치 순회
        for (int i = 0; i < roomItemSpawnPositions.Count && !reachedMax; i++)
        {
            var spawnPositions = roomItemSpawnPositions[i];
            if (spawnPositions.Count == 0) continue;

            // 해당 방의 드롭 설정
            var settings = roomSettings[i];
            int maxDrops = settings != null ? settings.maxDropCount : 0;
            int dropCount = Random.Range(1, maxDrops + 1);

            // 드롭 수만큼 아이템 선택
            for (int j = 0; j < dropCount; j++)
            {
                // 위치 선택
                int idx = Random.Range(0, spawnPositions.Count);
                Vector3Int cellPos = (Vector3Int)spawnPositions[idx];
                Vector3 worldPos = spreadTilemap.ItemSpawnTilemap
                                   .CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0f);
                Vector2 velocity = new Vector2(Random.Range(-5f, 5f), Random.Range(15f, 20f));

                // 타입 결정 (Use 25%, Normal 60%, Special 15%)
                float p = Random.value;
                ItemType selectedType = p < 0.25f ? ItemType.UseItem
                                      : p < 0.85f ? ItemType.Normal
                                      : ItemType.Special;

                // 후보군 & 가격 산정
                var candidates = itemList.Where(d => d.itemType == selectedType).ToList();
                if (candidates.Count == 0) continue;
                ItemData data = candidates[Random.Range(0, candidates.Count)];

                int price = Random.Range(data.priceRange.x, data.priceRange.y + 1);

                // ★ 여기에 합산 후 최대값 초과 시 중단
                if (totalPriceSum + price > maxTotalPriceSum)
                {
                    reachedMax = true;
                    break;
                }

                dropInfos.Add((data, worldPos, velocity, price));
                totalPriceSum += price;
            }
        }

        // 2) 총합 절대값 보정
        int targetSum = Mathf.Clamp(totalPriceSum, minTotalPriceSum, maxTotalPriceSum);
        float adjustRatio = (float)targetSum / Mathf.Max(totalPriceSum, 1);

        // 3) 최종 보정된 가격으로 실제 드롭 생성
        foreach (var (data, pos, vel, price) in dropInfos)
        {
            int adjustedPrice = Mathf.RoundToInt(price * adjustRatio);
            var invItem = new InventoryItem(data, adjustedPrice);
            var drop = Instantiate(dropPrefab, pos, Quaternion.identity, dropParent);
            drop.GetComponent<ItemObject>().SetupItem(invItem, vel);
        }

        Debug.Log($"[DropItems] OriginalSum: {totalPriceSum}, ClampedSum: {targetSum}");
    }

    /// <summary>
    /// 일정 시간마다 몬스터를 스폰하는 코루틴
    /// </summary>
    private IEnumerator SpawnMonstersRoutine()
    {
        while (true)
        {
            float rawHour = GameManager.instance.hours + GameManager.instance.minutes / 60f;
            // 7시(7)부터 22시(22)까지 정규화
            float timeNorm = Mathf.Clamp01((rawHour - 7f) / 15f);

            // 동적 스폰 간격 계산
            // 7시(timeNorm=0) → Min= maxSpawnInterval(30s), Max= maxSpawnInterval+20(50s)
            // 22시(timeNorm=1) → Min= minSpawnInterval(10s), Max= maxSpawnInterval(30s)
            float dynamicMin = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, timeNorm);
            float dynamicMax = Mathf.Lerp(maxSpawnInterval + 20f, maxSpawnInterval, timeNorm);

            float waitTime = Random.Range(dynamicMin, dynamicMax);
            Debug.Log($"[Spawn Interval] {rawHour:F1}h → {waitTime:F1}s 대기");

            yield return new WaitForSeconds(waitTime);
            SpawnMonsters();
        }
    }



    /// <summary>
    /// 실제로 한 사이클마다 몬스터를 1~3마리 스폰
    /// </summary>
    private void SpawnMonsters()
    {
        // 1) 시간 정규화 (8시→0, 22시→1)
        float rawHour = GameManager.instance.hours + GameManager.instance.minutes / 60f;
        float timeNorm = Mathf.Clamp01((rawHour - 8f) / 14f);
        float smoothTime = Mathf.Pow(timeNorm, 2);

        // 2) 스폰 마리 수 가중치 설정
        //    초반에는 1마리 우세, 말기로 갈수록 3마리 가중치 상승
        float cw1 = 1f - smoothTime;  // 1마리 가중치
        float cw2 = 1f;               // 2마리 가중치
        float cw3 = smoothTime * 2f;  // 3마리 가중치

        // 3) 룰렛으로 스폰 개수 결정
        float totalCW = cw1 + cw2 + cw3;
        float rc = Random.value * totalCW;
        int totalSpawnCount = rc < cw1 ? 1
                              : rc < cw1 + cw2 ? 2
                              : 3;
        Debug.Log(
            $"[Spawn Count] TimeNorm:{timeNorm:P2} " +
            $"Weights(1:{cw1:F2},2:{cw2:F2},3:{cw3:F2}) " +
            $"Selector:{rc:F2} → count:{totalSpawnCount}"
        );

        // 4) 플레이어 방 인덱스 계산
        Vector3 playerPos = _playerManager.PlayerObject.transform.position;
        Vector3Int playerCell = spreadTilemap.MonsterSpawnTilemap.WorldToCell(playerPos);
        int currentRoomIdx = -1;
        for (int i = 0; i < roomBounds.Count; i++)
            if (roomBounds[i].Contains(playerCell))
                currentRoomIdx = i;

        // 5) 실제 소환 루프
        int spawnedCount = 0;
        for (int s = 0; s < totalSpawnCount; s++)
        {
            // 후보 방 목록 (플레이어 방 제외)
            var candidates = new List<int>();
            for (int i = 0; i < roomMonsterSpawnPositions.Count; i++)
            {
                if (i == currentRoomIdx) continue;
                if (roomMonsterSpawnPositions[i].Count > 0 &&
                    roomSettings[i].monsterPrefabs.Length > 0)
                    candidates.Add(i);
            }
            if (candidates.Count == 0) break;

            // 무작위 방 선택 및 위치 계산
            int roomIdx = candidates[Random.Range(0, candidates.Count)];
            var positions = roomMonsterSpawnPositions[roomIdx];
            Vector2Int cell = positions[Random.Range(0, positions.Count)];
            Vector3 worldPos = spreadTilemap.MonsterSpawnTilemap
                .CellToWorld((Vector3Int)cell) + new Vector3(0.5f, 0.5f, 0f);

            // 가중치 룰렛으로 몬스터 프리팹 선택
            GameObject chosen = ChooseWeightedPrefab(roomSettings[roomIdx].monsterPrefabs, roomIdx);
            Instantiate(chosen, worldPos, Quaternion.identity, monsterParent);
            spawnedCount++;
            Debug.Log(
                $"[Spawned] #{spawnedCount} Room:{roomIdx} " +
                $"→ diff:{chosen.GetComponent<Enemy_Stats>().difficulty}"
            );
        }

        Debug.Log(
            $"[SpawnMonsters] Requested:{totalSpawnCount}, Spawned:{spawnedCount}"
        );
    }

    // 몬스터 프리팹 중 하나를 가중치 룰렛으로 선택
    private GameObject ChooseWeightedPrefab(GameObject[] prefabs, int roomIdx)
    {
        // 1) 깊이 정규화 (0 ~ 1)
        float centerY = roomBounds[roomIdx].min.y + roomBounds[roomIdx].size.y * 0.5f;
        float depthNorm = Mathf.Clamp01((mapMax.y - centerY) / (float)(mapMax.y - mapMin.y));

        // 2) 시간 정규화 (8시→0, 22시→1)
        float rawHour = GameManager.instance.hours + GameManager.instance.minutes / 60f;
        float timeNorm = Mathf.Clamp01((rawHour - 8f) / 14f);

        // 3) 난이도별 기본 가중치 및 보너스 파라미터
        float baseW1 = 3f;         // difficulty 1 기본
        float baseW2 = 2f;         // difficulty 2 기본
        float baseW3 = 0.05f;      // difficulty 3 기본

        // 레벨 2 보너스 (시간·깊이)
        float timeBonus2 = timeNorm * 1f;    // 최대 +1
        float depthBonus2 = depthNorm * 0.3f; // 최대 +0.3

        // 레벨 3 보너스 (시간·깊이)
        float timeBonus3 = timeNorm * 2f;    // 최대 +2
        float depthBonus3 = depthNorm * 0.5f; // 최대 +0.5

        // 4) 가중치 계산
        //    - w1: 시간이 흐를수록 줄어들도록
        //    - w2: 기본 + 시간·깊이 보너스
        //    - w3: 기본 + 시간·깊이 보너스
        float w1 = baseW1 * (1f - timeNorm);
        float w2 = baseW2 + timeBonus2 + depthBonus2;
        float w3 = baseW3 + timeBonus3 + depthBonus3;

        // 5) 룰렛용 가중치 맵 구성
        var weightMap = new Dictionary<GameObject, float>();
        foreach (var p in prefabs)
        {
            int diff = p.GetComponent<Enemy_Stats>().difficulty;
            weightMap[p] = (diff == 1 ? w1
                             : diff == 2 ? w2
                             : w3);
        }

        // 6) 디버그 로그: 난이도별 확률 계산
        float totalW = weightMap.Values.Sum();
        float p1 = weightMap.Where(kv => kv.Key.GetComponent<Enemy_Stats>().difficulty == 1)
                            .Sum(kv => kv.Value) / totalW;
        float p2 = weightMap.Where(kv => kv.Key.GetComponent<Enemy_Stats>().difficulty == 2)
                            .Sum(kv => kv.Value) / totalW;
        float p3 = weightMap.Where(kv => kv.Key.GetComponent<Enemy_Stats>().difficulty == 3)
                            .Sum(kv => kv.Value) / totalW;
        Debug.Log($"[Spawn Prob] Room:{roomIdx} Depth:{depthNorm:P2} Time:{timeNorm:P2} → D1:{p1:P2} D2:{p2:P2} D3:{p3:P2}");

        // 7) 룰렛으로 하나 선택
        float roll = Random.value * totalW;
        float acc = 0f;
        foreach (var kv in weightMap)
        {
            acc += kv.Value;
            if (roll < acc)
                return kv.Key;
        }

        // 안전장치: 만약 룰렛이 실패하면 첫번째 리턴
        return prefabs.Length > 0 ? prefabs[0] : null;
    }


    private void Instantiate_Player()
    {
        GameObject prefab = Character_Manager.instance.currentCharacter.characterPrefab;
        GameObject player = Instantiate(prefab, player_SpawnPos.position, Quaternion.identity);
        player.transform.position = player_SpawnPos.position;
        _playerManager.PlayerObject = player;
    }

    private void SettingManager()
    {
        _camera = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CinemachineCamera>();
        _camera.Follow = _playerManager.PlayerObject.transform;
        _camera.LookAt = _playerManager.PlayerObject.transform;

        GameManager.instance.isSetting = true;
    }

    private void PlaceRoom(GameObject roomPrefab, Vector2Int offset)
    {
        GetTilemaps(roomPrefab,
                out Tilemap floorTM,
                out Tilemap wallTM,
                out Tilemap corridorTM,
                out Tilemap itemSpawnTM,
                out Tilemap monsterSpawnTM
        );
        CopyTilemapWithTiles(floorTM, offset, floorTiles, floorTileDict);
        CopyTilemapWithTiles(wallTM, offset, wallTiles, wallTileDict);
        CopyTilemapWithTiles(corridorTM, offset, corridorTiles, corridorTileDict);
        CopyTilemapWithTiles(itemSpawnTM, offset, itemSpawnTiles, itemSpawnTileDict);
        CopyTilemapWithTiles(monsterSpawnTM, offset, monsterSpawnTiles, monsterSpawnTileDict);

        var localBounds = floorTM.cellBounds;
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

        GetTilemaps(roomPrefab, out Tilemap floorTM, out Tilemap wallTM, out Tilemap corridorTM, out Tilemap itemSpawnTM, out Tilemap monsterSpawnTM);
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

    private void GetTilemaps(GameObject roomPrefab, out Tilemap floorTM, out Tilemap wallTM, out Tilemap corridorTM, out Tilemap itemSpawnTM, out Tilemap monsterSpawnTM)
    {
        var children = roomPrefab.GetComponentsInChildren<Transform>();
        Transform parent = children[1];  // 프로젝트 구조에 따라 인덱스를 조정하세요
        floorTM = parent.Find("FloorTilemap").GetComponent<Tilemap>();
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
