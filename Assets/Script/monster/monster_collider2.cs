using UnityEngine;

public class monster_collider2 : MonoBehaviour
{
    public Transform player;  // �÷��̾� ��ü�� Transform
    public Transform enemy;   // �� ��ü�� Transform

    void Update()
    {
        // �÷��̾�� �� ���� �Ÿ� ���
        float distance = Vector3.Distance(player.position, enemy.position);

        // �Ÿ� ��� (������)
        Debug.Log("Player and Enemy Distance: " + distance);
    }
}
