using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class MimicBoxco : MonoBehaviour
{

    [SerializeField]  public float detectionRadius = 1f; // ���� �÷��̾ ������ ����
    [SerializeField]  private Transform Player; // �÷��̾��� ��ġ
    [SerializeField]  public float moveSpeed = 3f; // ���� �̵� �ӵ�
    [SerializeField] public float jumpspeed;
    [SerializeField] public float jumpcooltime;
    public bool Hide = true;
    public float distanceMax = 5f;

    bool Scoping = false;
    int moverandomDirection; //�����¿�������� �̵��ϴ� ����
    float Concealment_time;
    bool Moving =  false;
    float Moving_Time;
    float distance;
    float move_time = 5f;

    public float xDistanceThreshold = 2f;

    private Transform Floor_Measurement;
    private Transform attack;
    private Transform ragne;


    public HillDetection HillDetection;

    private GameObject Floor_Measurement_pos;

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
        /*Floor_Measurement_pos = GetComponent<Rigidbody2D>();*/
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
                    //StartCoroutine("Move", move_time);
                    
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
                //float distanceX = Mathf.Abs(player.position.x - transform.position.x);

                //// X�� �������� ���� �Ÿ� �̻� ���̳� ���� �̵�
                //if (distanceX > xDistanceThreshold)
                //{
                // �÷��̾� �������� X�����θ� �̵�
                Chase();
                //Debug.Log("�Ѵ´�!");
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
            //Debug.Log("�÷��̾ ���� ������ �������ϴ�!");
        }
    }
    private void Move()
    {
        
        if (Moving == true )
        { if(distance < distanceMax)
            {
                transform.position += new Vector3(moverandomDirection * moveSpeed * Time.deltaTime, 0f, 0f);
                distance += Time.deltaTime;
            }
            else 
            {
                Moving = false;

            }
        }
        


        


        
    }
    private void Chase()
    {
        float moveDirection = Player.position.x > transform.position.x ? 1f : -1f; // �÷��̾ �������̸� 1, �����̸� -1
        transform.position += new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);

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

        
        

        
        Vector3 newPosition = Floor_Measurement.transform.position;
        
        /*newPosition = Floor_Measurement.transform.position;*/
        newPosition.x = moveDirection == -1f && newPosition.x > 0f ? -1f : 1f ;
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


        Floor_Measurement.transform.position = newPosition;
        Debug.Log(newPosition.x);
        
        //newPosition.x *= -1f;






        //newPosition.y = ;
        //Floor_Measurement.position = 
        //Vector2 newPosition = moveDirection;
        //Floor_Measurement.position = Vector3(newPosition, transform.position);
        //rb.Floor_Measurement = new Vector2(moveDirection * currentSpeed / (isAttack ? 2 : 1), rb.linearVelocityY);
    }
}