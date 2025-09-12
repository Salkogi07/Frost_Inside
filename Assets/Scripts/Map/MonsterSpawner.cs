using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class MonsterSpawner : NetworkBehaviour
{
    [Header("몬스터 스폰 간격 설정")]
    [Tooltip("다음 스폰까지 최소 대기 시간(초)")]
    [SerializeField] private float minSpawnInterval = 10f;
    [Tooltip("다음 스폰까지 최대 대기 시간(초)")]
    [SerializeField] private float maxSpawnInterval = 30f;

    [Header("동적 난이도 설정")]
    [Tooltip("이 값 이상일 때 '중간' 난이도 몬스터가 등장합니다.")]
    [Range(0, 1)]
    [SerializeField] private float mediumDifficultyThreshold = 0.3f;
    [Tooltip("이 값 이상일 때 '어려움' 난이도 몬스터가 등장합니다.")]
    [Range(0, 1)]
    [SerializeField] private float hardDifficultyThreshold = 0.7f;
    
    [Tooltip("난이도 점수 계산 시 시간의 영향력 가중치")]
    [SerializeField] private float timeDifficultyWeight = 0.6f;
    [Tooltip("난이도 점수 계산 시 맵 깊이의 영향력 가중치")]
    [SerializeField] private float depthDifficultyWeight = 0.4f;

    private MakeRandomMap mapData;
    private SpreadTilemap spreadTilemap;

    private void Awake()
    {
        GameManager.instance.monsterSpawner = this;
    }

    public void InitializeAndStartSpawning(MakeRandomMap generatedMap, SpreadTilemap tilemapController)
    {
        if (!IsServer) return;
        this.mapData = generatedMap;
        this.spreadTilemap = tilemapController;
        Debug.Log("[MonsterSpawner] Initialization complete. Starting spawn routine.");
        StartCoroutine(SpawnMonstersRoutine());
    }

    private IEnumerator SpawnMonstersRoutine()
    {
        if (!IsServer) yield break;
        while (true)
        {
            // ... (스폰 간격 계산 로직은 동일) ...
            float rawHour = TimerManager.instance.Hours + TimerManager.instance.Minutes / 60f;
            float timeNorm = Mathf.Clamp01((rawHour - 7f) / 15f);
            float dynamicMin = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, timeNorm);
            float dynamicMax = Mathf.Lerp(maxSpawnInterval + 20f, maxSpawnInterval, timeNorm);
            float waitTime = Random.Range(dynamicMin, dynamicMax);
            Debug.Log($"[Spawn Interval] {rawHour:F1}h → {waitTime:F1}s 대기");
            yield return new WaitForSeconds(waitTime);
            SpawnMonsters();
        }
    }

    private void SpawnMonsters()
    {
        if (!IsServer || mapData == null || NetworkEnemyPool.Instance == null) return;

        // ... (스폰 수 결정 및 플레이어 방 제외 로직은 동일) ...
        float rawHour = TimerManager.instance.Hours + TimerManager.instance.Minutes / 60f;
        float timeNorm = Mathf.Clamp01((rawHour - 8f) / 14f);
        int totalSpawnCount = DetermineSpawnCount(timeNorm);
        HashSet<int> occupiedRoomIndices = GetOccupiedRooms();

        // --- 핵심 수정 부분 ---

        // 1. 풀에서 스폰 가능한 모든 몬스터 프리팹의 마스터 리스트를 가져옵니다.
        var allAvailablePrefabs = NetworkEnemyPool.Instance.GetAvailablePrefabs();
        if (allAvailablePrefabs == null || allAvailablePrefabs.Count == 0)
        {
            Debug.LogError("[MonsterSpawner] NetworkEnemyPool has no prefabs to spawn!");
            return;
        }

        int spawnedCount = 0;
        for (int s = 0; s < totalSpawnCount; s++)
        {
            // 2. 스폰 위치를 정하기 위해 유효한 방 후보를 찾습니다.
            //    (이제 난이도 조건 없이, 스폰 위치가 있는지만 확인합니다.)
            var candidateRooms = Enumerable.Range(0, mapData.RoomMonsterSpawnPositions.Count)
                .Where(i => !occupiedRoomIndices.Contains(i) && mapData.RoomMonsterSpawnPositions[i].Count > 0)
                .ToList();
            
            if (candidateRooms.Count == 0)
            {
                Debug.LogWarning("[MonsterSpawner] No valid rooms to spawn monsters.");
                break;
            }

            int roomIdx = candidateRooms[Random.Range(0, candidateRooms.Count)];
            
            // 3. 선택된 방의 위치(깊이)와 현재 시간을 기반으로 '위협도 점수'를 계산합니다.
            float centerY = mapData.RoomBounds[roomIdx].center.y;
            float depthNorm = Mathf.Clamp01((mapData.MapMax.y - centerY) / (float)(mapData.MapMax.y - mapData.MapMin.y));
            float threatLevel = Mathf.Clamp01(timeNorm * timeDifficultyWeight + depthNorm * depthDifficultyWeight);

            // 4. 위협도 점수에 따라 이번 스폰에서 허용될 난이도 목록을 동적으로 생성합니다.
            var dynamicAllowedDifficulties = new List<int>();
            if (threatLevel >= 0f) dynamicAllowedDifficulties.Add(1); // 난이도 1은 항상 가능
            if (threatLevel >= mediumDifficultyThreshold) dynamicAllowedDifficulties.Add(2);
            if (threatLevel >= hardDifficultyThreshold) dynamicAllowedDifficulties.Add(3);
            
            Debug.Log($"[Difficulty] Room:{roomIdx} Time:{timeNorm:P1} Depth:{depthNorm:P1} -> Threat:{threatLevel:P1}. Allowed Diffs: [{string.Join(",", dynamicAllowedDifficulties)}]");

            // 5. 생성된 난이도 목록으로 풀의 프리팹들을 필터링합니다.
            var spawnablePrefabs = allAvailablePrefabs
                .Where(p => p.GetComponent<Stats.Enemy_Stats>() != null && dynamicAllowedDifficulties.Contains(p.GetComponent<Stats.Enemy_Stats>().difficulty))
                .ToList();

            if (spawnablePrefabs.Count == 0) continue;

            // 6. 스폰 위치를 정하고, 필터링된 프리팹 목록에서 가중치에 따라 하나를 선택하여 스폰합니다.
            var positions = mapData.RoomMonsterSpawnPositions[roomIdx];
            Vector2Int cell = positions[Random.Range(0, positions.Count)];
            Vector3 worldPos = spreadTilemap.MonsterSpawnTilemap
                .CellToWorld((Vector3Int)cell) + new Vector3(0.5f, 0.5f, 0f);

            GameObject chosenPrefab = ChooseWeightedPrefab(spawnablePrefabs, timeNorm, depthNorm);
            if (chosenPrefab == null) continue;
            
            var monsterNetworkObject = NetworkEnemyPool.Instance.GetObject(chosenPrefab, worldPos, Quaternion.identity);
            if (monsterNetworkObject != null)
            {
                var enemyScript = monsterNetworkObject.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.InitializeForPool(worldPos);
                }
                spawnedCount++;
            }
        }
        Debug.Log($"[SpawnMonsters] Requested:{totalSpawnCount}, Spawned:{spawnedCount}");
    }

    // 스폰 수 결정 및 플레이어 방 찾는 로직을 별도 함수로 분리하여 가독성 향상
    private int DetermineSpawnCount(float timeNorm)
    {
        float smoothTime = Mathf.Pow(timeNorm, 2);
        float cw1 = 1f - smoothTime, cw2 = 1f, cw3 = smoothTime * 2f;
        float totalCW = cw1 + cw2 + cw3;
        float rc = Random.value * totalCW;
        return rc < cw1 ? 1 : rc < cw1 + cw2 ? 2 : 3;
    }
    
    private HashSet<int> GetOccupiedRooms()
    {
        var occupiedRoomIndices = new HashSet<int>();
        List<GameObject> allPlayers = PlayerDataManager.instance.GetAllPlayerObjects();
        if (allPlayers == null) return occupiedRoomIndices;
        
        foreach (var player in allPlayers.Where(p => p != null))
        {
            Vector3Int playerCellPos = Vector3Int.FloorToInt(player.transform.position);
            for (int i = 0; i < mapData.RoomBounds.Count; i++)
            {
                if (mapData.RoomBounds[i].Contains(playerCellPos))
                {
                    occupiedRoomIndices.Add(i);
                    break;
                }
            }
        }
        return occupiedRoomIndices;
    }

    // 파라미터로 timeNorm과 depthNorm을 직접 받도록 수정하여 중복 계산 방지
    private GameObject ChooseWeightedPrefab(List<GameObject> prefabs, float timeNorm, float depthNorm)
    {
        if (prefabs == null || prefabs.Count == 0) return null;

        float baseW1 = 3f, baseW2 = 2f, baseW3 = 0.05f;
        float timeBonus2 = timeNorm * 1f, depthBonus2 = depthNorm * 0.3f;
        float timeBonus3 = timeNorm * 2f, depthBonus3 = depthNorm * 0.5f;
        float w1 = baseW1 * (1f - timeNorm);
        float w2 = baseW2 + timeBonus2 + depthBonus2;
        float w3 = baseW3 + timeBonus3 + depthBonus3;

        var weightMap = new Dictionary<GameObject, float>();
        foreach (var p in prefabs)
        {
            int diff = p.GetComponent<Stats.Enemy_Stats>().difficulty;
            weightMap[p] = (diff == 1 ? w1 : diff == 2 ? w2 : w3);
        }
        
        float totalW = weightMap.Values.Sum();
        if (totalW == 0) return prefabs[0];
        float roll = Random.value * totalW;
        float acc = 0f;
        foreach (var kv in weightMap)
        {
            acc += kv.Value;
            if (roll < acc) return kv.Key;
        }
        return prefabs[0];
    }
}