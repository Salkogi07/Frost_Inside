using UnityEngine;
using UnityEngine.EventSystems;


public class MimicBox : MonoBehaviour
{

    public float detectionRadius = 5f; // ���� �÷��̾ ������ ����
    public Transform player; // �÷��̾��� ��ġ
    public float moveSpeed = 3f; // ���� �̵� �ӵ�
    public bool Hide = true;
    bool Scoping;
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
    

    // �÷��̾ �����ϴ� �Լ�


    // �� �����Ӹ��� �÷��̾���� �Ÿ� üũ
    private void Update()
    {
        /*if(Scoping == false && Pattern != pattern.Concealment)
        {
            
            float Concealment_time  = Time.deltaTime;
            if(Concealment_time >= 5f)
            {
                Pattern = pattern.Concealment;
            }
        }
        switch (Pattern)
        {
         
            case pattern.Concealment:
                if(Scoping == false)
                {
                    //�ٷ� �����
                    Scoping = true;
                }
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

                Pattern =  pattern.Move;
                Scoping = false;
                break;
            case pattern.Move:
                
                
                    Move();
                
                
                
                break;

            case pattern.Chase:
                //float distanceX = Mathf.Abs(player.position.x - transform.position.x);

                //// X�� �������� ���� �Ÿ� �̻� ���̳� ���� �̵�
                //if (distanceX > xDistanceThreshold)
                //{
                // �÷��̾� �������� X�����θ� �̵�
                Chase();

                //Vector3 direction = (player.position - transform.position).normalized;
                //transform.position += direction * moveSpeed * Time.deltaTime;



                break;

            case pattern.Attack:

                break;
            default:

                break;



        }*/
        
    } 
    private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && Hide == false ) // �÷��̾� �±׸� ���� ��ü ����
            {
                Pattern = pattern.Chase;
                Scoping = true;
        }

        }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Scoping = false;
            //Debug.Log("�÷��̾ ���� ������ �������ϴ�!");
        }
    }
    private void Move()
    {
        if ()
        {

        
        moverandomDirection = Random.Range(1, 2) == 1 ? 1 : -1;


        }else if (Scoping == false)
        {
            transform.position += new Vector3(moverandomDirection * moveSpeed * Time.deltaTime, 0f, 0f);
        }
        
    
    }
    private void Chase()
    {
        float moveDirection = player.position.x > transform.position.x ? 1f : -1f; // �÷��̾ �������̸� 1, �����̸� -1
        transform.position += new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);
    }
}