using UnityEngine;

public class monster_AI : MonoBehaviour
{
    public Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //detectionCollider.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        //if (isChasing && Player != null)
        //{
            // 플레이어 방향으로 이동
            //Vector3 direction = player.position - transform.position;
            //direction.Normalize();
            //transform.position += direction * 2f * Time.deltaTime;
            transform.position = new Vector3(-1f,0,0);
        //}
    }
}
