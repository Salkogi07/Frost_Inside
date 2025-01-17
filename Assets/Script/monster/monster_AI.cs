using System;
using Unity.VisualScripting;
using UnityEngine;

public class monster_AI : MonoBehaviour
{

    public float detectionRadius = 5f; // 적이 플레이어를 감지할 범위
    public Transform player; // 플레이어의 위치
    public float moveSpeed = 3f; // 적의 이동 속도
    string pattern ;
    public float xDistanceThreshold = 2f;

    // 범위 내에 플레이어가 들어왔을 때 호출
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 플레이어 태그를 가진 객체 감지
        {
            pattern = "chase";
            Debug.Log("플레이어가 범위 안에 들어왔습니다!");
        }
    }

    // 범위 내에 플레이어가 계속 있을 때 호출
    //private void OnTriggerStay2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        pattern = "chase";
    //        // 적이 플레이어를 추적
    //    }
    //}

    // 범위 밖으로 나가면 호출
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pattern = "";
            Debug.Log("플레이어가 범위 밖으로 나갔습니다!");
        }
    }

    // 플레이어를 추적하는 함수


    // 매 프레임마다 플레이어와의 거리 체크
    private void Update()
    {
        
        switch (pattern)
        {
            case "move":

                break;

            case "chase":
                //float distanceX = Mathf.Abs(player.position.x - transform.position.x);

                //// X축 기준으로 일정 거리 이상 차이날 때만 이동
                //if (distanceX > xDistanceThreshold)
                //{
                    // 플레이어 방향으로 X축으로만 이동
                    float moveDirection = player.position.x > transform.position.x ? 1f : -1f; // 플레이어가 오른쪽이면 1, 왼쪽이면 -1
                    transform.position += new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);
                
                //Vector3 direction = (player.position - transform.position).normalized;
                //transform.position += direction * moveSpeed * Time.deltaTime;
                break;

            case "attack":

                break;
            default:

                break;
        }
    }
}
