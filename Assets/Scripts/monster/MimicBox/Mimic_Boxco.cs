using System.Collections;
using UnityEngine;


public class MimicBoxDirector : MonoBehaviour
{
    [Header ("Mimic stats")]
    [SerializeField]  public float detectionRadius = 1f; 

    [SerializeField] private Transform Player;
    float move_time = 5f;

    float moveDirection;
    public float distanceMax = 5f;
    public bool Hide = true;
    bool Scoping = false;
    int moverandomDirection;
    float Concealment_time;
    bool Moving =  false;
    float Moving_Time;
    float distance;
    bool jumping = false;
    bool e = true;
    public float xDistanceThreshold = 2f;
    float yDistance;
    bool attacking;
    
    
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
    private Collision_Conversion collisions;

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
        Floor_Measurement = transform.Find("Floor_Measurement");
        rb = GetComponent<Rigidbody2D>();
        attack = transform.Find("attack");
        ragne = transform.Find("check");
        HillDetection = transform.Find("HillDetection");
        FloorMeasurement = FloorMeasurement.GetComponent<Floor_Measurement>();
        collisions = GetComponent<Collision_Conversion>();
        //Player = GameObject.FindWithTag("Player").transform;

        Monster_Jump = GetComponent<Monster_Jump>();
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
    
    private void Update()
    {
        Debug.Log(attacking);
        if (e)
        {
            //Debug.Log("dldi");
            Player = GameObject.FindWithTag("Player").transform;
            e = !e;
        }
        if (Scoping == false && Pattern != pattern.Concealment)
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
                    Hide = true;
                    Debug.Log("������!");
                }
                //if("")
                //{
                //    Pattern = pattern.exiting;
                //}
                break;
            case pattern.exiting:
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
                //Debug.Log("장병준");
                break;
            case pattern.Chase:
                Chase();
                direction();
                Moving_Time = 0f;
                break;
            case pattern.Attack:
                attacking = true;
                //Mimic_Attack.hits();
                StartCoroutine(attackingAnimationToEnd());
                attacking = false;
                break;
            default:
                break;
        }
    } 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !Hide && !attacking)
        {
            Pattern = pattern.Chase;
            Scoping = true;
            Moving = false;
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Player") && !Hide && !attacking)
        {
            Pattern = pattern.Chase;
            yDistance = Player.position.y - transform.position.y;
            if (yDistance > 1f && Monster_Jump.jump_cooltime <= 0f)
            {
                Monster_Jump.OnJump();
                Monster_Jump.jump_cooltime = 5f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !Hide && !attacking)
        {
            Scoping = false;
            Pattern = pattern.Move;
        }
    }
    private void Move()
    {
        if (Moving)
        { if(distance < distanceMax)
            {
                transform.position += new Vector3(moverandomDirection * stat.MonsterGroup.speed * Time.deltaTime, 0f, 0f);
                distance += stat.MonsterGroup.speed * Time.deltaTime;
                hillDetections.CheckForHillAhead();
                collisions.Collision_conversion();
                if (collisions.IsCollision)
                {
                    moverandomDirection = -moverandomDirection;
                    collisions.IsCollision = false;
                }
                if (!FloorMeasurement.Groundcheck && !Monster_Jump.isJumping)
                {
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
            moveDirection = Player.position.x > transform.position.x ? 1f : -1f;
        }
        if (moveDirection == -1f)
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
    
    private void Chase()
    {
        //FloorMeasurement.direction = moveDirection;

        transform.position += new Vector3(moveDirection * stat.MonsterGroup.speed * Time.deltaTime, 0f, 0f);
        hillDetections.CheckForHillAhead();
    }

    IEnumerator attackingAnimationToEnd()
    {
        //animator.SetTrigger("Attack");
        
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        //yield return new WaitForSeconds(stateInfo.length);
        yield return new WaitForSeconds(1f);
        
        Debug.Log("빙칠링!");
    }
}