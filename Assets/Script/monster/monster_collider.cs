using UnityEngine;

public class monster_collider : MonoBehaviour
{
    private bool playerInRange = false; 
    private Transform Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("1");
        Player = GameObject.FindWithTag("Player").transform;
    }
void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) // �÷��̾� �±׸� ���� ��ü ����
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
            // ���� �÷��̾ ����
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
