using UnityEngine;

public class monster_collider2 : MonoBehaviour
{
    private Transform player;  // �÷��̾� ��ü�� Transform
    public Transform enemy;   // �� ��ü�� Transform

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }
    void Update()
    {
        // �÷��̾�� �� ���� �Ÿ� ���
        float distance = Vector3.Distance(player.position, enemy.position);
        if (distance < 3f)
        {
            

        }

        // �Ÿ� ��� (������)
        if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Player and Enemy Distance: " + distance);
        }
        
    }
}
