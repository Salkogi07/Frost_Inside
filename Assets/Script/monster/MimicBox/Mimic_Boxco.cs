using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class MimicBoxDirector : MonoBehaviour
{
    [Header ("Mimic stats")]
    [SerializeField]  public float detectionRadius = 1f; // ���� �÷��̾ ������ ����
    [SerializeField]  private Transform Player; // �÷��̾��� ��ġ
    [SerializeField] public float jumpspeed = 5f;
    [SerializeField] public float jumpcooltime;
    [SerializeField] public float jumpingmax;
    float move_time = 5f;


    float moveDirection;
    public float distanceMax = 5f;
    public bool Hide = true;
    bool Scoping = false;
    int moverandomDirection; //�����¿�������� �̵��ϴ� ����
    float Concealment_time;
    bool Moving =  false;
    float Moving_Time;
    float distance;
    bool jumping = false;

    public float xDistanceThreshold = 2f;
    float yDistance;

    private Transform Floor_Measurement;
    private Transform attack;
    private Transform ragne;


    public Mimic_attack Mimic_Attack;
    public HillDetection HillDetection;
    public Floor_Measurement FloorMeasurement;

    private GameObject Floor_Measurement_pos;
    private Monster_stat stat;
    private Monster_Jump Monster_Jump;

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

    private void Awake()
    {
        stat = GetComponent<Monster_stat>();
        Floor_Measurement = transform.Find("Floor Measurement");
        rb = GetComponent<Rigidbody2D>();
        attack = transform.Find("Attack");
        ragne = transform.Find("GameObjeck");
        FloorMeasurement = FloorMeasurement.GetComponent<Floor_Measurement>();
        Player = GameObject.FindWithTag("Player").transform;

        Monster_Jump = GetComponent<Monster_Jump>();
            Debug.Log("�س���");
        
    }
    void Start()
    {
        
        //attack = attack.GetComponent<Mimic_attack>();
        //Player = GameObject.FindWithTag("Player").transform; 

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
        yDistance = Player.position.y - transform.position.y;
        if(yDistance> 2f && Monster_Jump.jump_cooltime <= 0f)
        {
            Monster_Jump.OnJump();
            Monster_Jump.jump_cooltime = 5f;
        }// ����,���� �ʿ�

        if(Scoping == false && Pattern != pattern.Concealment)
        {
            
            Concealment_time  += Time.deltaTime;
            if(Concealment_time >= 30f)
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

                /*Debug.Log("ddeee");*/

                
                    Move();
                
                if (Moving == false && Moving_Time >= 5f)
                
                    
                {
                    moverandomDirection = Random.Range(1, 3) == 1 ? 1 : -1;
                    
                    
                    Moving_Time = 0f;
                    Moving = true;
                    distance = 0f;
                }
                else if(Moving == false)
                {
                    Moving_Time += Time.deltaTime;
                }
                
                //Debug.Log("�����δ�!");

                    // ���� �ʿ� (5���� ������ ���ϰ� �����Ÿ� �̵��ʿ�)

                break;

            case pattern.Chase:
                
                Chase();
                direction();
                


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
            //Debug.Log("�÷��̾ ���� ������ �������ϴ�!");
        }
    }
    private void Move()
    {
        
        if (Moving)
        { if(distance < distanceMax)
            {
                transform.position += new Vector3(moverandomDirection * stat.speed * Time.deltaTime, 0f, 0f);
                distance += Time.deltaTime;
                direction();
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
            moveDirection = moverandomDirection == 1 ? 1f : -1f ;
        }


        if (Pattern == pattern.Chase)
        {
            moveDirection = Player.position.x > transform.position.x ? 1f : -1f; // �÷��̾ �������̸� 1, �����̸� -1
        }
        if (moveDirection == -1f) //���� �ʿ�
        {
            Floor_Measurement.position = new Vector3(transform.position.x - 1f, Floor_Measurement.position.y, 0f);
            attack.position = new Vector3(transform.position.x - stat.range[0], attack.position.y, 0f);
        }
        else
        {
            Floor_Measurement.position = new Vector3(transform.position.x + 1f, Floor_Measurement.position.y, 0f);
            attack.position = new Vector3(transform.position.x + stat.range[0], attack.position.y, 0f);
        }
       
    


    }
    private void jump()
    {
        
        jumping = true;
        if(jumpingmax >= 1f)
        {
            transform.position += new Vector3(0f, jumpspeed, 0f);
            jumpingmax += Time.deltaTime;
        }
        
    }
    private void Chase()
    {
        
        //FloorMeasurement.direction = moveDirection;
        transform.position += new Vector3(moveDirection * stat.speed * Time.deltaTime, 0f, 0f);

        if(HillDetection.Groundcheck == true)
        {
            transform.position += new Vector3(0f, 0.1f, 0f);
        }
        else
        {

        }

        Vector3 newFloorPosition = Floor_Measurement.position;
        
        

        
        

        //Debug.Log(newPosition);
        
        
        //Debug.Log(newPosition.x);
        //bool minus = moveDirection == -1f && newPosition.x > 0f ? true : false;
        //newPosition.x = minus == true ? 1f : 2f;
        
        /*newPosition = Floor_Measurement.transform.position;*/
        //newPosition.x = moveDirection == -1f && newPosition.x > 0f ? -1f : 1f ;
        
        


        /*Debug.Log(newPosition.x);*/

        /*newPosition.y -= 1f;
        if (moveDirection == -1f)
        {

            newPosition.x -= 1f;
        }
        else
        {
            newPosition.x += 1f;
        }*/
        //Floor_Measurement.transform.position = newPosition;
        //Debug.Log(newPosition.x);
        
        //newPosition.x *= -1f;
        //newPosition.y = ;
        //Floor_Measurement.position = 
        //Vector2 newPosition = moveDirection;
        //Floor_Measurement.position = Vector3(newPosition, transform.position);
        //rb.Floor_Measurement = new Vector2(moveDirection * currentSpeed / (isAttack ? 2 : 1), rb.linearVelocityY);
    }
}