using System;
using UnityEngine;

public class monster_AI : MonoBehaviour
{

    public float detectionRadius = 5f; // ���� �÷��̾ ������ ����
    public Transform player; // �÷��̾��� ��ġ
    public float moveSpeed = 3f; // ���� �̵� �ӵ�

    private bool playerInRange = false;

    // ���� ���� �÷��̾ ������ �� ȣ��
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // �÷��̾� �±׸� ���� ��ü ����
        {
            playerInRange = true;
            Debug.Log("�÷��̾ ���� �ȿ� ���Խ��ϴ�!");
        }
    }

    // ���� ���� �÷��̾ ��� ���� �� ȣ��
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // ���� �÷��̾ ����
            ChasePlayer();
        }
    }

    // ���� ������ ������ ȣ��
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("�÷��̾ ���� ������ �������ϴ�!");
        }
    }

    // �÷��̾ �����ϴ� �Լ�
    private void ChasePlayer()
    {
        if (playerInRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // �� �����Ӹ��� �÷��̾���� �Ÿ� üũ
    private void Update()
    {
        if (playerInRange)
        {
            // �÷��̾ ���� �ȿ� ���� �� �ٸ� ������ �߰��� �� �ֽ��ϴ�.
        }
    }
}
