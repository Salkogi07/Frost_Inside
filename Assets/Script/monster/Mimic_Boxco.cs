using Mirror.Examples.CouchCoop;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UIElements;


public class MimicBoxco : MonoBehaviour
{

    [SerializeField]  public float detectionRadius = 1f; // ���� �÷��̾ ������ ����
    [SerializeField]  private Transform Player; // �÷��̾��� ��ġ
    [SerializeField]  public float moveSpeed = 3f; // ���� �̵� �ӵ�
    public bool Hide = true;
    bool Scoping = true;
    int moverandomDirection; //�����¿�������� �̵��ϴ� ����
    float Concealment_time;
    bool Moving =  false;


    float move_time = 5f;

    public float xDistanceThreshold = 2f;

    private Transform Floor_Measurement;
    private Transform attack;
    private Transform ragne;
    public Rigidbody2D rb { get; private set; }
    public enum pattern
    {
        Move,
        Attack,
        Concealment,
        Chase,
        exiting,



    }
    public pattern Pattern = pattern.Concealment;


    void Start()
    {
        //Player = GetComponent<Rigidbody2D>();
        Floor_Measurement = transform.Find("Floor Measurement");
        rb = GetComponent<Rigidbody2D>();
        attack = transform.Find("Attack");
        ragne = transform.Find("GameObjeck");
        //Player = GameObject.FindWithTag("Player").transform; 
    }
    

    

    // ���� ������ ������ ȣ��
    

    // �÷��̾ �����ϴ� �Լ�


    // �� �����Ӹ��� �÷��̾���� �Ÿ� üũ
    private void Update()
    {

        if(Scoping == false && Pattern != pattern.Concealment)
        {
            
            Concealment_time  = +Time.deltaTime;
            if(Concealment_time >= 5f)
            {
                Pattern = pattern.Concealment;
            }
        }
        else
        {
            Concealment_time = 0f;
        }
        switch (Pattern)
        {
         
            case pattern.Concealment:
                if(Scoping == false)
                {
                    //�ٷ� �����
                    Hide = true;
                    Debug.Log("������!");
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

                
                if(Moving == false)
                {
                    moverandomDirection = Random.Range(1, 2) == 1 ? 1 : -1;
                    //StartCoroutine("Move", move_time);
                    //Move(,move_time);
                    Moving = true;
                }
                
                //Debug.Log("�����δ�!");


                break;

            case pattern.Chase:
                //float distanceX = Mathf.Abs(player.position.x - transform.position.x);

                //// X�� �������� ���� �Ÿ� �̻� ���̳� ���� �̵�
                //if (distanceX > xDistanceThreshold)
                //{
                // �÷��̾� �������� X�����θ� �̵�
                Chase();
                Debug.Log("�Ѵ´�!");
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
                Pattern = pattern.Chase;
                Scoping = true;
        }

        }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Hide == false)
        {
            Scoping = false;
            Pattern = pattern.Move;
            Debug.Log("�÷��̾ ���� ������ �������ϴ�!");
        }
    }
    void Move()
    {

        for (int i = 0; i < 120; i++)
        {
            if (Scoping == false)// ������Ʈ ������ �ٴ��� ������ �̵� ������ �ߴ�
            {

                transform.position += new Vector3(moverandomDirection * moveSpeed * Time.deltaTime, 0f, 0f);
            }
            //else if (Scoping == true)
            //{
            //continue;
            //}

            
        }

        Moving = false;

    }
    private void Chase()
    {
        float moveDirection = Player.position.x > transform.position.x ? 1f : -1f; // �÷��̾ �������̸� 1, �����̸� -1
        transform.position += new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);
        //Vector3 newPosition = Floor_Measurement.position;
        //newPosition.x = moveDirection;
        //Floor_Measurement.position = Floor_Measurement.position-newPosition;
        //Vector2 newPosition = moveDirection;
        //Floor_Measurement.position -= new Vector3(0,1f);
        //rb.linearVelocity = new Vector2(moveDirection * currentSpeed / (isAttack ? 2 : 1), rb.linearVelocityY);
    }
}