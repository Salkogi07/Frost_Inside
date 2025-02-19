using UnityEngine;
using UnityEngine.EventSystems;
using static monster_AI;

public class MimicBox : MonoBehaviour
{

    public float detectionRadius = 5f; // ���� �÷��̾ ������ ����
    public Transform player; // �÷��̾��� ��ġ
    public float moveSpeed = 3f; // ���� �̵� �ӵ�
    bool Hide = true;

    public float xDistanceThreshold = 2f;

    public enum pattern
    {
        Move,
        Attack,
        Concealment,
        Chase,


    }
    public pattern Pattern = pattern.Concealment;


    private void Start()
    {

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // �÷��̾� �±׸� ���� ��ü ����
        {
            //pattern = "chase";
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
            //pattern = "move";
            Debug.Log("�÷��̾ ���� ������ �������ϴ�!");
        }
    }

    // �÷��̾ �����ϴ� �Լ�


    // �� �����Ӹ��� �÷��̾���� �Ÿ� üũ
    private void Update()
    {

        switch (Pattern)
        {

            case pattern.Concealment:

                //�÷��̾ �ǵ��� ������¸� ����
                //if("")
                //{
                //    Hide = false;
                //}

                break;
            //case :

            //    break;
            case pattern.Move:
                
                Move();
                break;

            case pattern.Chase:
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

            case pattern.Attack:

                break;
            default:

                break;



        }
        
    }
    private void Move()
    {
        int moverandomDirection = Random.Range(1, 2) == 1 ? 1 : -1;
        transform.position += new Vector3(moverandomDirection * moveSpeed * Time.deltaTime, 0f, 0f);
    
    }
}