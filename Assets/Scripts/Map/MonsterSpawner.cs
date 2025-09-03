using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class MonsterSpawner : NetworkBehaviour
{
    [Header("몬스터 부모 설정")]
    [SerializeField] private Transform monsterParent;

    [Header("몬스터 스폰 간격 설정")]
    [Tooltip("다음 스폰까지 최소 대기 시간(초)")]
    [SerializeField] private float minSpawnInterval = 10f;
    [Tooltip("다음 스폰까지 최대 대기 시간(초)")]
    [SerializeField] private float maxSpawnInterval = 30f;
    
    // 맵 생성 후 전달받을 데이터
    private MakeRandomMap mapData;
    private SpreadTilemap spreadTilemap; // 몬스터 위치 계산용

    /// <summary>
    /// (서버 전용) 맵 생성 데이터를 받아 스폰을 시작하는 초기화 함수.
    /// </summary>
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
        // 서버가 아니면 즉시 코루틴 종료
        if (!IsServer) yield break;

        while (true)
        {
            float rawHour = GameManager.instance.Hours + GameManager.instance.Minutes / 60f;
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
        if (!IsServer || mapData == null) return;
        
        float rawHour = GameManager.instance.Hours + GameManager.instance.Minutes / 60f;
        float timeNorm = Mathf.Clamp01((rawHour - 8f) / 14f);
        float smoothTime = Mathf.Pow(timeNorm, 2);

        float cw1 = 1f - smoothTime;
        float cw2 = 1f;
        float cw3 = smoothTime * 2f;

        float totalCW = cw1 + cw2 + cw3;
        float rc = Random.value * totalCW;
        int totalSpawnCount = rc < cw1 ? 1 : rc < cw1 + cw2 ? 2 : 3;

        Debug.Log($"[Spawn Count] TimeNorm:{timeNorm:P2} Weights(1:{cw1:F2},2:{cw2:F2},3:{cw3:F2}) Selector:{rc:F2} → count:{totalSpawnCount}");

        // 플레이어 위치는 서버에서 직접 찾아야 함
        // (단순화를 위해 이 예제에서는 플레이어 위치 추적 로직은 생략. 필요 시 추가해야 함)
        int currentRoomIdx = -1; // 플레이어가 있는 방 인덱스 (계산 로직 필요)

        int spawnedCount = 0;
        for (int s = 0; s < totalSpawnCount; s++)
        {
            var candidates = new List<int>();
            for (int i = 0; i < mapData.RoomMonsterSpawnPositions.Count; i++)
            {
                if (i == currentRoomIdx) continue;
                if (mapData.RoomMonsterSpawnPositions[i].Count > 0 &&
                    mapData.RoomSettings[i].monsterPrefabs.Length > 0)
                    candidates.Add(i);
            }
            if (candidates.Count == 0) break;

            int roomIdx = candidates[Random.Range(0, candidates.Count)];
            var positions = mapData.RoomMonsterSpawnPositions[roomIdx];
            Vector2Int cell = positions[Random.Range(0, positions.Count)];
            Vector3 worldPos = spreadTilemap.MonsterSpawnTilemap
                .CellToWorld((Vector3Int)cell) + new Vector3(0.5f, 0.5f, 0f);

            GameObject chosen = ChooseWeightedPrefab(mapData.RoomSettings[roomIdx].monsterPrefabs, roomIdx);
            
            // 네트워크 오브젝트로 몬스터 스폰
            GameObject monsterInstance = Instantiate(chosen, worldPos, Quaternion.identity, monsterParent);
            monsterInstance.GetComponent<NetworkObject>().Spawn(true);
            
            spawnedCount++;
            Debug.Log($"[Spawned] #{spawnedCount} Room:{roomIdx} → diff:{chosen.GetComponent<Stats.Enemy_Stats>().difficulty}");
        }

        Debug.Log($"[SpawnMonsters] Requested:{totalSpawnCount}, Spawned:{spawnedCount}");
    }
    
    private GameObject ChooseWeightedPrefab(GameObject[] prefabs, int roomIdx)
    {
        if (mapData == null) return prefabs.Length > 0 ? prefabs[0] : null;

        float centerY = mapData.RoomBounds[roomIdx].min.y + mapData.RoomBounds[roomIdx].size.y * 0.5f;
        float depthNorm = Mathf.Clamp01((mapData.MapMax.y - centerY) / (float)(mapData.MapMax.y - mapData.MapMin.y));

        float rawHour = GameManager.instance.Hours + GameManager.instance.Minutes / 60f;
        float timeNorm = Mathf.Clamp01((rawHour - 8f) / 14f);

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
        if (totalW == 0) return prefabs.Length > 0 ? prefabs[0] : null;

        float roll = Random.value * totalW;
        float acc = 0f;
        foreach (var kv in weightMap)
        {
            acc += kv.Value;
            if (roll < acc) return kv.Key;
        }

        return prefabs.Length > 0 ? prefabs[0] : null;
    }
}