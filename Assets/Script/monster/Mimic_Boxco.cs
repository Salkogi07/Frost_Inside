using Mirror.Examples.CouchCoop;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;


public class MimicBoxco : MonoBehaviour
{

    [SerializeField]  public float detectionRadius = 1f; // ���� �÷��̾ ������ ����
    [SerializeField]  private Transform Player; // �÷��̾��� ��ġ
    [SerializeField]  public float moveSpeed = 3f; // ���� �̵� �ӵ�
    public bool Hide = true;

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

                Debug.Log("ddeee");

                if (distance <5f)
                {
                    Move();
                    distance = Time.deltaTime;
                }
                else
                {
                    Moving = true;
                }
                    if (Moving == false && Moving_Time >= 5f)
                {
                    moverandomDirection = Random.Range(1, 2) == 1 ? 1 : -1;
                    //StartCoroutine("Move", move_time);
                    
                    Moving_Time = 0f;
                    Moving = true;
                }
                else
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
    private void Move()
    {
        transform.position += new Vector3(moverandomDirection * moveSpeed * Time.deltaTime, 0f, 0f);
        //while(distance< 40f)
        //{
        //    if (Scoping == false )// ������Ʈ ������ �ٴ��� ������ �̵� ������ �ߴ�
        //    {
        //        distance += Time.time;
        //        Debug.Log("" + distance);
        //        
        //    }//else if (Scoping == true)
        //    //{
        //    //continue;
        //    //}
        //}


        //distance = 0f;
        //Moving = false;
        //Moving_Time = 0f;
    }
    private void Chase()
    {
        float moveDirection = Player.position.x > transform.position.x ? 1f : -1f; // �÷��̾ �������̸� 1, �����̸� -1
        transform.position += new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);
        Vector3 newPosition = Floor_Measurement.position;
        newPosition.x = moveDirection;
        //newPosition.y = transform.ps;
        Floor_Measurement.position = newPosition;
        //Floor_Measurement.position = 
        //Vector2 newPosition = moveDirection;
        //Floor_Measurement.position = Vector3(newPosition, transform.position);
        //rb.linearVelocity = new Vector2(moveDirection * currentSpeed / (isAttack ? 2 : 1), rb.linearVelocityY);
    }
}