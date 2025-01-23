using System;
using Unity.VisualScripting;
using UnityEngine;

public class monster_AI2 : MonoBehaviour
{

    public float detectionRadius = 5f; // ���� �÷��̾ ������ ����
    public Transform player; // �÷��̾��� ��ġ
    public float moveSpeed = 3f; // ���� �̵� �ӵ�
    string pattern ;
    public float xDistanceThreshold = 2f;

    // ���� ���� �÷��̾ ������ �� ȣ��
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // �÷��̾� �±׸� ���� ��ü ����
        {
            pattern = "chase";
            Debug.Log("�÷��̾ ���� �ȿ� ���Խ��ϴ�!");
        }
    }

    // ���� ���� �÷��̾ ��� ���� �� ȣ��
    //private void OnTriggerStay2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        pattern = "chase";
    //        // ���� �÷��̾ ����
    //    }
    //}

    // ���� ������ ������ ȣ��
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pattern = "";
            Debug.Log("�÷��̾ ���� ������ �������ϴ�!");
        }
    }

    // �÷��̾ �����ϴ� �Լ�


    // �� �����Ӹ��� �÷��̾���� �Ÿ� üũ
    private void Update()
    {
        
        switch (pattern)
        {
            case "move":

                break;

            case "chase":
                //float distanceX = Mathf.Abs(player.position.x - transform.position.x);

                //// X�� �������� ���� �Ÿ� �̻� ���̳� ���� �̵�
                //if (distanceX > xDistanceThreshold)
                //{
                    // �÷��̾� �������� X�����θ� �̵�
                    float moveDirection = player.position.x > transform.position.x ? 1f : -1f; // �÷��̾ �������̸� 1, �����̸� -1
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
