using System.Collections;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

public class Underground_Ghost : MonoBehaviour
{
    [Header("Underground_Ghost stats")]
    [SerializeField] public float detectionRadius = 1f; // ���� �÷��̾ ������ ���� 

    [SerializeField] private Transform Player; // �÷��̾��� ��ġ
    float move_time = 5f;

    float moveDirection;
    public float distanceMax = 5f;
    bool Scoping = false;
    float moverandomDirection_x; //�����¿�������� �̵��ϴ� ����
    float moverandomDirection_y; //�������Ϲ������� �̵��ϴ� ����
    float Concealment_time;
    bool Moving = false;
    float Moving_Time;
    float distance;
    bool jumping = false;
    bool e = true;
    public float xDistanceThreshold = 2f;
    float yDistance;
    bool attacking = false;
    Vector2 directions;



    private Transform ragne;
    


    public Ghost_attack Ghost_Attack;
    //public HillDetection hillDetections;
    //public Floor_Measurement FloorMeasurement;

    //private GameObject Floor_Measurement_pos;
    private Monster_stat stat;
    //private Monster_Jump Monster_Jump;
    //private Collision_Conversion collisions;

    public Rigidbody2D rb { get; private set; }

    public enum pattern
    {
        Move,
        Attack,
        Chase,



    }
    public pattern Pattern = pattern.Move;

    private void Awake()
    {
        stat = GetComponent<Monster_stat>();
        //Floor_Measurement = transform.Find("Floor_Measurement");
        rb = GetComponent<Rigidbody2D>();
        //attack = transform.Find("attack");
        ragne = transform.Find("check");
        //HillDetection = transform.Find("HillDetection");
        //FloorMeasurement = FloorMeasurement.GetComponent<Floor_Measurement>();
        //collisions = GetComponent<Collision_Conversion>();
        Player = GameObject.FindWithTag("Player").transform;


        Debug.Log("�س���");

    }
    void Start()
    {

        //attack = attack.GetComponent<Mimic_attack>();
        //Player = GetComponent<>

        Player = Player.GetComponent<Transform>();
        //if(Monster_Jump != null)
        //{
        //Monster_Jump.OnJump();
        //}

    }




    // ���� ������ ������ ȣ��


    // �÷��̾ �����ϴ� �Լ�


    // �� �����Ӹ��� �÷��̾���� �Ÿ� üũ
    private void Update()
    {
        //Debug.Log(attacking);
        if (e)
        {
            //Debug.Log("dldi");
            Player = GameObject.FindWithTag("Player").transform;
            e = !e;
        }
        
        
        switch (Pattern)
        {

            
            //case :

            //    break;
            
            case pattern.Move:

                /*Debug.Log("ddeee");*/


                Move();

                if (Moving == false && Moving_Time >= 5f)


                {
                    moverandomDirection_x = Random.Range(-1f, 1f);
                    moverandomDirection_y = Random.Range(-1f, 1f);
                    directions = new Vector2(moverandomDirection_x, moverandomDirection_y).normalized;


                    Moving_Time = 0f;
                    Moving = true;
                    distance = 0f;
                }
                else if (Moving == false)
                {
                    Moving_Time += Time.deltaTime;
                }

                //Debug.Log("�����δ�!");

                // ���� �ʿ� (5���� ������ ���ϰ� �����Ÿ� �̵��ʿ�)

                break;

            case pattern.Chase:

                Chase();
                direction();
                Moving_Time = 0f;


                break;

            case pattern.Attack:

                attacking = true;
                //�ִϸ��̼�
                //Mimic_Attack.hits();
                StartCoroutine(attackingAnimationToEnd());
                attacking = false;

                //�����ʿ�
                break;
            default:

                break;



        }

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !attacking) // �÷��̾� �±׸� ���� ��ü ����
        {
            
            Pattern = pattern.Chase;
            Scoping = true;
            Moving = false;
        }

    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !attacking)
        {
            Pattern = pattern.Chase;
            //Debug.Log("Stay");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !attacking)
        {
            Scoping = false;
            Pattern = pattern.Move;
            
        }
    }
    private void Move()
    {

        if (Moving)
        {
            if (distance < distanceMax)
            {

                

               
                direction();
                // �̵�

                transform.position += (Vector3)(directions * stat.speed * Time.deltaTime);
                //distance += stat.speed * Time.deltaTime;
                distance += Time.deltaTime;
                
                

                

            }
            else
            {
                Moving = false;

            }
        }







    }

    private void direction()
    {
        if (Pattern == pattern.Move)
        {
            Debug.Log(directions);
        }


        if (Pattern == pattern.Chase)
        {
            directions = (Player.position - transform.position).normalized;
            
        }
        //if (moveDirection == -1f) //���� �ʿ�
        //{
        //    Floor_Measurement.position = new Vector3(transform.position.x - 1f, Floor_Measurement.position.y, 0f);
        //    attack.position = new Vector3(transform.position.x - stat.range[0], attack.position.y, 0f);
        //    HillDetection.position = new Vector3(transform.position.x - (hillDetections.boxSize[0] * 3f), HillDetection.position.y, 0f);
        //}
        //else
        //{
        //    Floor_Measurement.position = new Vector3(transform.position.x + 1f, Floor_Measurement.position.y, 0f);
        //    attack.position = new Vector3(transform.position.x + stat.range[0], attack.position.y, 0f);
        //    HillDetection.position = new Vector3(transform.position.x + (hillDetections.boxSize[0] * 3f), HillDetection.position.y, 0f);
        //}




    }

    private void Chase()
    {

        //FloorMeasurement.direction = moveDirection;

        //transform.position += new Vector3(moveDirection * stat.speed * Time.deltaTime, 0f, 0f);
        //hillDetections.CheckForHillAhead();
        

        // �̵�
        transform.position += (Vector3)(directions * stat.speed * Time.deltaTime);








    }

    IEnumerator attackingAnimationToEnd()
    {
        //animator.SetTrigger("Attack");

        //// ���� ���� ���� ��������
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // �ִϸ��̼� ���̸�ŭ ���
        //yield return new WaitForSeconds(stateInfo.length);
        yield return new WaitForSeconds(1f);
        // ���� ���� �ڵ� ����
        Debug.Log("�ִϸ��̼� ������!");
    }
}
