using System;
using UnityEngine;

public class monster_AI : MonoBehaviour
{

    public float detectionRadius = 5f; // 적이 플레이어를 감지할 범위
    public Transform player; // 플레이어의 위치
    public float moveSpeed = 3f; // 적의 이동 속도

    private bool playerInRange = false;

    // 범위 내에 플레이어가 들어왔을 때 호출
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 플레이어 태그를 가진 객체 감지
        {
            playerInRange = true;
            Debug.Log("플레이어가 범위 안에 들어왔습니다!");
        }
    }

    // 범위 내에 플레이어가 계속 있을 때 호출
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // 적이 플레이어를 추적
            ChasePlayer();
        }
    }

    // 범위 밖으로 나가면 호출
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("플레이어가 범위 밖으로 나갔습니다!");
        }
    }

    // 플레이어를 추적하는 함수
    private void ChasePlayer()
    {
        if (playerInRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // 매 프레임마다 플레이어와의 거리 체크
    private void Update()
    {
        if (playerInRange)
        {
            // 플레이어가 범위 안에 있을 때 다른 반응을 추가할 수 있습니다.
        }
    }
}
