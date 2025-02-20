using UnityEngine;
using UnityEngine.EventSystems;
using static monster_AI;

public class MimicBox : MonoBehaviour
{

    public float detectionRadius = 5f; // ���� �÷��̾ ������ ����
    public Transform player; // �÷��̾��� ��ġ
    public float moveSpeed = 3f; // ���� �̵� �ӵ�
    public bool Hide = true;
    int moverandomDirection; //������������ �̵��ϴ� ����

    public float xDistanceThreshold = 2f;

    public enum pattern
    {
        Move,
        Attack,
        Concealment,
        Chase,
        exiting,



    }
    public pattern Pattern = pattern.Concealment;


    private void Start()
    {

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
                //    Pattern = pattern.exiting;
                //}

                break;
            //case :

            //    break;
            case pattern.exiting:
                //������ �ִϸ��̼�
                break;
            case pattern.Move:
                moverandomDirection = Random.Range(1, 2) == 1 ? 1 : -1;
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
    private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && Hide == false ) // �÷��̾� �±׸� ���� ��ü ����
            {
                
                
            }

        }
    private void Move()
    {
        
        
        transform.position += new Vector3(moverandomDirection * moveSpeed * Time.deltaTime, 0f, 0f);
    
    }
}