using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager instance;

    [Tooltip("이 씬에서 사용될 스폰 지점들의 Transform 배열")]
    [SerializeField] private Transform[] spawnPoints;

    private int nextSpawnPointIndex = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두 개 이상의 SpawnPointManager가 존재합니다. 하나를 파괴합니다.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 다음 스폰 지점의 Transform을 순서대로 반환합니다.
    /// </summary>
    public Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("SpawnPointManager에 스폰 지점이 할당되지 않았습니다! 매니저 자신의 위치를 반환합니다.");
            return transform; // 비상시 자기 자신의 위치를 반환
        }

        Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
        nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;
        return spawnPoint;
    }
}