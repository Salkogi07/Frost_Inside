using UnityEngine;

public class monster_collider : MonoBehaviour
{
    private bool playerInRange = false; 
    private Transform Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("1");
    }
private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) // 플레이어 태그를 가진 객체 감지
            {
                playerInRange = true;
                Debug.Log("Enter");
            }
        }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // 적이 플레이어를 추적
            Debug.Log("Stay");
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Exit");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(Player = null)
        {
            Debug.Log("aa");
        }
    }
}
