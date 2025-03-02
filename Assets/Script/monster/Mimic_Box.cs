using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MimicBox : MonoBehaviour
{

    [SerializeField]  public float detectionRadius = 5f; // 적이 플레이어를 감지할 범위
    [SerializeField]  private Transform Player; // 플레이어의 위치
    [SerializeField]  public float moveSpeed = 3f; // 적의 이동 속도
    public bool Hide = true;
    bool Scoping = true;
    int moverandomDirection; //랜덤좌우방향으로 이동하는 변수
    float Concealment_time;


    float move_time = 5f;

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


    void Start()
    {
        //Player = GetComponent<Rigidbody2D>();
        Transform Floor_Measurement = transform.Find("Floor Measurement");
        Player = GameObject.FindWithTag("Player").transform;
    } 
    

    // 범위 내에 플레이어가 계속 있을 때 호출
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pattern = pattern.Chase;
            // 적이 플레이어를 추적
        }
    }

    // 범위 밖으로 나가면 호출
    

    // 플레이어를 추적하는 함수


    // 매 프레임마다 플레이어와의 거리 체크
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
                    //바로 숨어듬
                    Hide = true;
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

                moverandomDirection = Random.Range(1, 2) == 1 ? 1 : -1;
                //StartCoroutine(Move("Move", move_time));



                break;

            case pattern.Chase:
                //float distanceX = Mathf.Abs(player.position.x - transform.position.x);

                //// X축 기준으로 일정 거리 이상 차이날 때만 이동
                //if (distanceX > xDistanceThreshold)
                //{
                // 플레이어 방향으로 X축으로만 이동
                Chase();

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
            if (other.CompareTag("Player") && Hide == false ) // 플레이어 태그를 가진 객체 감지
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
            Debug.Log("플레이어가 범위 밖으로 나갔습니다!");
        }
    }
    //private IEnumerator Move()
    //{
    //    for (int i = 0; i < 120; i++)
    //    {
    //        //if()// 오브젝트 주위에 바닥이 있으면 이동 없으면 중단
    //        //{
    //        //    transform.position += new Vector3(moverandomDirection * moveSpeed * Time.deltaTime, 0f, 0f);
    //        //}
    //        //else if ()
    //        //{

    //        //}


    //    }
        
        
    
    //}
    private void Chase()
    {
        float moveDirection = Player.position.x > transform.position.x ? 1f : -1f; // 플레이어가 오른쪽이면 1, 왼쪽이면 -1
        transform.position += new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);
    }
}