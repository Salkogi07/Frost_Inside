using System.Collections;
using UnityEngine;


public class MimicBoxDirector : MonoBehaviour
{
    [Header ("Mimic stats")]
    [SerializeField]  public float detectionRadius = 1f; // 적이 플레이어를 감지할 범위
    [SerializeField]  private Transform Player; // 플레이어의 위치
    [SerializeField] public float jumpspeed = 5f;
    [SerializeField] public float jumpcooltime;
    [SerializeField] public float jumpingmax;
    float move_time = 5f;

    float moveDirection;
    public float distanceMax = 5f;
    public bool Hide = true;
    bool Scoping = false;
    int moverandomDirection; //랜덤좌우방향으로 이동하는 변수
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
    private Transform HillDetection;


    public Mimic_attack Mimic_Attack;
    public HillDetection hillDetections;
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
        HillDetection = transform.Find("HillDetection");
        FloorMeasurement = FloorMeasurement.GetComponent<Floor_Measurement>();
        //Player = GameObject.FindWithTag("Player").transform;

        Monster_Jump = GetComponent<Monster_Jump>();
            Debug.Log("해냇따");
        
    }
    void Start()
    {
        
        //attack = attack.GetComponent<Mimic_attack>();
        Player = GameObject.FindWithTag("Player").transform; 

        //if(Monster_Jump != null)
        //{
        //Monster_Jump.OnJump();
        //}
        
    }
    

    

    // 범위 밖으로 나가면 호출
    

    // 플레이어를 추적하는 함수


    // 매 프레임마다 플레이어와의 거리 체크
    private void Update()
    {   
        

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
                    //바로 숨어듬
                    Hide = true;
                    Debug.Log("숨었다!");
                }
                //플레이어가 건들어야 숨기상태를 해제
                //if("")
                //{
                //    Pattern = pattern.exiting;
                //}

                break;
            //case :

            //    break;
            case pattern.exiting:
                //나오는 애니메이션

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
                
                //Debug.Log("움직인다!");

                    // 수정 필요 (5초후 방향을 정하고 일정거리 이동필요)

                break;

            case pattern.Chase:
                
                Chase();
                direction();
                Moving_Time = 0f;


                break;

            case pattern.Attack:


                break;
            default:

                break;



        }
        
    } 
    private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !Hide ) // 플레이어 태그를 가진 객체 감지
            {
            
                Pattern = pattern.Chase;
                Scoping = true;
                Moving = false;
        }

        }
    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Player") && !Hide)
        {
            yDistance = Player.position.y - transform.position.y;
            if (yDistance > 1f && Monster_Jump.jump_cooltime <= 0f)
            {
                Monster_Jump.OnJump();
                Monster_Jump.jump_cooltime = 5f;
            }// 수정,정리 필요
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !Hide)
        {
            Scoping = false;
            Pattern = pattern.Move;
            //Debug.Log("플레이어가 범위 밖으로 나갔습니다!");
        }
    }
    private void Move()
    {
        
        if (Moving)
        { if(distance < distanceMax)
            {
                
                if(FloorMeasurement.Groundcheck)
                {
                    transform.position += new Vector3(moverandomDirection * stat.speed * Time.deltaTime, 0f, 0f);
                distance += Time.deltaTime;
                    hillDetections.CheckForHillAhead();
                }
                else { 
                    moverandomDirection = -moverandomDirection;

                }
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
            moveDirection = Player.position.x > transform.position.x ? 1f : -1f; // 플레이어가 오른쪽이면 1, 왼쪽이면 -1
        }
        if (moveDirection == -1f) //수정 필요
        {
            Floor_Measurement.position = new Vector3(transform.position.x - 1f, Floor_Measurement.position.y, 0f);
            attack.position = new Vector3(transform.position.x - stat.range[0], attack.position.y, 0f);
            HillDetection.position = new Vector3(transform.position.x - (hillDetections.boxSize[0]* 3f),HillDetection.position.y, 0f);
        }
        else
        {
            Floor_Measurement.position = new Vector3(transform.position.x + 1f, Floor_Measurement.position.y, 0f);
            attack.position = new Vector3(transform.position.x + stat.range[0], attack.position.y, 0f);
            HillDetection.position = new Vector3(transform.position.x+ (hillDetections.boxSize[0] *3f), HillDetection.position.y, 0f);
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
        hillDetections.CheckForHillAhead();









    }
}