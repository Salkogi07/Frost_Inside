using UnityEngine;

public class monster_collider2 : MonoBehaviour
{
    public Transform player;  // 플레이어 객체의 Transform
    public Transform enemy;   // 적 객체의 Transform

    void Update()
    {
        // 플레이어와 적 간의 거리 계산
        float distance = Vector3.Distance(player.position, enemy.position);

        // 거리 출력 (디버깅용)
        Debug.Log("Player and Enemy Distance: " + distance);
    }
}
