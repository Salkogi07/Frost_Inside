using System.Collections;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

public class Underground_Ghost : MonoBehaviour
{
    [Header("Underground_Ghost stats")]
    [SerializeField] public float detectionRadius = 1f; // 적이 플레이어를 감지할 범위 

    [SerializeField] private Transform Player; // 플레이어의 위치
    float move_time = 5f;

    float moveDirection;
    public float distanceMax = 5f;
    bool Scoping = false;
    float moverandomDirection_x; //랜덤좌우방향으로 이동하는 변수
    float moverandomDirection_y; //랜덤상하방향으로 이동하는 변수
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


        Debug.Log("해냇따");

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




    // 범위 밖으로 나가면 호출


    // 플레이어를 추적하는 함수


    // 매 프레임마다 플레이어와의 거리 체크
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

                //Debug.Log("움직인다!");

                // 수정 필요 (5초후 방향을 정하고 일정거리 이동필요)

                break;

            case pattern.Chase:

                Chase();
                direction();
                Moving_Time = 0f;


                break;

            case pattern.Attack:

                attacking = true;
                //애니매이션
                //Mimic_Attack.hits();
                StartCoroutine(attackingAnimationToEnd());
                attacking = false;

                //수정필요
                break;
            default:

                break;



        }

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !attacking) // 플레이어 태그를 가진 객체 감지
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
                // 이동
                float move_speed = stat.speed + (stat.speed * 50 / 100);
                transform.position += (Vector3)(directions * move_speed * Time.deltaTime);
                Debug.Log(move_speed);
                distance += stat.speed * Time.deltaTime;
                //distance += Time.deltaTime;
                
                

                

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
        //if (moveDirection == -1f) //수정 필요
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
        

        // 이동
        transform.position += (Vector3)(directions * stat.speed * Time.deltaTime);








    }

    IEnumerator attackingAnimationToEnd()
    {
        //animator.SetTrigger("Attack");

        //// 현재 상태 정보 가져오기
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 애니메이션 길이만큼 대기
        //yield return new WaitForSeconds(stateInfo.length);
        yield return new WaitForSeconds(1f);
        // 이제 다음 코드 실행
        Debug.Log("애니메이션 끝났다!");
    }
}
