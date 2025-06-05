using System.Collections;
using UnityEngine;

public class White_Parasite_Director: MonoBehaviour
{
    [Header("Mimic stats")]
    [SerializeField] public float detectionRadius = 1f;
    [SerializeField] private Transform Player;
    
    float move_time = 5f;

    float moveDirection;
    public float distanceMax = 5f;
    int moverandomDirection;
    float Concealment_time;
    bool Moving = false;
    bool monig= false;  
    float Moving_Time;
    float distance;
    bool jumping = false;
    bool e = true;

    public float xDistanceThreshold = 2f;
    float yDistance;
    float xDistance;

    private Transform Floor_Measurement;
    private Transform attack;
    private Transform ragne;
    private Transform HillDetection;


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
        Chase,
    }
    public pattern Pattern = pattern.Move;

    private void Awake()
    {
        stat = GetComponent<Monster_stat>();
        Floor_Measurement = transform.Find("Floor_Measurement");
        rb = GetComponent<Rigidbody2D>();
        attack = transform.Find("Attack");
        ragne = transform.Find("GameObjeck");
        HillDetection = transform.Find("HillDetection");
        FloorMeasurement = FloorMeasurement.GetComponent<Floor_Measurement>();
        //Player = GameObject.FindWithTag("Player").transform;

        collisions = GetComponent<Collision_Conversion>();  
        Monster_Jump = GetComponent<Monster_Jump>();
        Debug.Log("sdsds");
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
    
    private void Update()
    {
        if (e)
        {
            //Debug.Log("dldi");
            Player = GameObject.FindWithTag("Player").transform;
            e = !e;
        }

        switch (Pattern)
        {
            case pattern.Move:
                /*Debug.Log("ddeee");*/
                if (Moving)
                {
                    Move();
                }
                else if (Moving == false && Moving_Time >= 3f)
                {
                    moverandomDirection = Random.Range(1, 3) == 1 ? 1 : -1;
                    Moving = true;
                }
                else if (Moving == false)
                {
                    Moving_Time += Time.deltaTime;
                }
                break;
            case pattern.Chase:
                Moving = false;
                Chase();
                direction();
                //StartCoroutine(direction_chase());
                Moving_Time = 0f;
                break;
            case pattern.Attack:
                Destroy(gameObject);
                break;
            default:
                break;
        }

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pattern = pattern.Chase;
            Moving = false;
        }

    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            yDistance = Player.position.y - transform.position.y;
            xDistance = Player.position.x - transform.position.x;
            //Debug.Log(xDistance);
            //Debug.Log(yDistance);
            if (Monster_Jump.jump_cooltime <= 0f && !Monster_Jump.jumping)
            {
              if(yDistance > 1f || (xDistance < 1f && xDistance > -1f))
              {
                    Monster_Jump.OnJump();
                    Monster_Jump.jump_cooltime = 5f;
              }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pattern = pattern.Move;
        }
    }
    private void Move()
    {
        transform.position += new Vector3(moverandomDirection * stat.speed * Time.deltaTime, 0f, 0f);
        distance += stat.speed * Time.deltaTime;
        //if(!hillDetections.Operation)
        //{
        hillDetections.CheckForHillAhead();
        //}
                   
        if(!Monster_Jump.isJumping)
        {
            collisions.Collision_conversion();
        }
                    
        if(collisions.IsCollision)
        {
            moverandomDirection = -moverandomDirection;
            collisions.IsCollision = false;
        }

        if (!FloorMeasurement.Groundcheck && !Monster_Jump.isJumping)
        {
            moverandomDirection = -moverandomDirection;
        }
        
        direction();
        //else
        //{
        //    Moving = false;

        //}
    }

    private void direction()
    {
        if (Pattern == pattern.Move)
        {
            moveDirection = moverandomDirection == 1 ? 1f : -1f;
        }

        if (Pattern == pattern.Chase)
        {
            moveDirection = Player.position.x > transform.position.x ? 1f : -1f;
        }

        if (moveDirection == -1f)
        {
            Floor_Measurement.position = new Vector3(transform.position.x - 1f, Floor_Measurement.position.y, 0f);
            //attack.position = new Vector3(transform.position.x - stat.range[0], attack.position.y, 0f);
            HillDetection.position = new Vector3(transform.position.x - (hillDetections.boxSize[0] * 1.5f), HillDetection.position.y, 0f);
        }
        else
        {
            Floor_Measurement.position = new Vector3(transform.position.x + 1f, Floor_Measurement.position.y, 0f);
            //attack.position = new Vector3(transform.position.x + stat.range[0], attack.position.y, 0f);
            HillDetection.position = new Vector3(transform.position.x + (hillDetections.boxSize[0] * 1.5f), HillDetection.position.y, 0f);
        }
    }

    /*private IEnumerator direction_chase()
    {
        if (Pattern == pattern.Chase)
        {
            yield return new WaitForSeconds(0.3f);
            moveDirection = Player.position.x > transform.position.x ? 1f : -1f; //  ÷  ̾        ̸  1,      ̸  -1
        }
    }*/
    
    private void Chase()
    {
        //FloorMeasurement.direction = moveDirection;

        transform.position += new Vector3(moveDirection * stat.speed * Time.deltaTime, 0f, 0f);
        hillDetections.CheckForHillAhead();
    }
}