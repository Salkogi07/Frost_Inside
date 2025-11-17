using UnityEngine;
using Unity.Netcode;
using System.Collections;
using R3;

public class OutSideMonsterSpawner : NetworkBehaviour
{
    [Header("스폰 기준 오브젝트")]
    [Tooltip("이 오브젝트의 위치를 기준으로 몬스터를 스폰합니다.")]
    [SerializeField] private Transform spawnCenterTarget;

    [Header("스폰 범위 설정")]
    [Tooltip("기준 오브젝트로부터 좌우로 스폰될 최대 거리입니다.")]
    [SerializeField] private float spawnRangeX = 10f;

    [Header("스폰 주기 설정")]
    [Tooltip("몬스터 스폰 주기의 랜덤 범위를 설정합니다. X = 최소 시간, Y = 최대 시간")]
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(2f, 5f);
    
    private bool isSpawning = false;

    private void OnValidate()
    {
        if (spawnIntervalRange.x < 0) spawnIntervalRange.x = 0;
        if (spawnIntervalRange.y < 0) spawnIntervalRange.y = 0;
        
        if (spawnIntervalRange.x > spawnIntervalRange.y)
        {
            spawnIntervalRange.x = spawnIntervalRange.y;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        TimerManager.instance.HoursObservable
            .Subscribe(hour => OnHourChanged(hour))
            .AddTo(this);
    }

    private void OnHourChanged(int hour)
    {
        if (hour >= 12 && !isSpawning)
        {
            if (spawnCenterTarget == null)
            {
                Debug.LogError("스폰 기준 오브젝트(Spawn Center Target)가 지정되지 않았습니다!", this.gameObject);
                return;
            }
            isSpawning = true;
            StartCoroutine(SpawnMonsterRoutine());
        }
    }

    private IEnumerator SpawnMonsterRoutine()
    {
        while (isSpawning)
        {
            float randomInterval = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSeconds(randomInterval);

            var availablePrefabs = NetworkOutSideEnemyPool.Instance.GetAvailablePrefabs();
            if (availablePrefabs == null || availablePrefabs.Count == 0)
            {
                Debug.LogWarning("스폰할 수 있는 몬스터 프리팹이 없습니다.");
                continue;
            }

            // 기준 오브젝트의 현재 위치를 가져옵니다.
            Vector3 targetPosition = spawnCenterTarget.position;

            // 기준 오브젝트의 x 위치에서 -spawnRangeX ~ +spawnRangeX 사이의 랜덤한 오프셋을 계산합니다.
            float randomOffsetX = Random.Range(-spawnRangeX, spawnRangeX);
            
            // 최종 스폰 위치를 계산합니다. Y와 Z값은 기준 오브젝트의 값을 그대로 사용합니다.
            Vector3 spawnPosition = new Vector3(targetPosition.x + randomOffsetX, targetPosition.y, targetPosition.z);

            int randomPrefabIndex = Random.Range(0, availablePrefabs.Count);
            GameObject monsterPrefab = availablePrefabs[randomPrefabIndex];
            
            if (monsterPrefab != null)
            {
                NetworkOutSideEnemyPool.Instance.GetObject(monsterPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    // 씬 뷰에서 스폰 범위를 시각적으로 보여주는 기즈모를 그립니다.
    private void OnDrawGizmosSelected()
    {
        // 기준 오브젝트가 설정되어 있어야 기즈모를 그립니다.
        if (spawnCenterTarget == null) return;

        Gizmos.color = Color.cyan;

        // 기준 오브젝트의 위치를 가져옵니다.
        Vector3 targetPosition = spawnCenterTarget.position;
        
        // 스폰 범위의 시작점과 끝점을 계산합니다.
        Vector3 startPoint = new Vector3(targetPosition.x - spawnRangeX, targetPosition.y, targetPosition.z);
        Vector3 endPoint = new Vector3(targetPosition.x + spawnRangeX, targetPosition.y, targetPosition.z);
        
        // 기준 오브젝트의 Y 레벨에 맞춰 스폰 범위를 선으로 그립니다.
        Gizmos.DrawLine(startPoint, endPoint);
    }
}