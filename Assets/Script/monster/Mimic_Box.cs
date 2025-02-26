using UnityEngine;
using UnityEngine.EventSystems;


public class MimicBox : MonoBehaviour
{

    [SerializeField]  public float detectionRadius = 5f; // ���� �÷��̾ ������ ����
    [SerializeField]  private Transform Player; // �÷��̾��� ��ġ
    [SerializeField]  public float moveSpeed = 3f; // ���� �̵� �ӵ�
    public bool Hide = true;
    bool Scoping = true;
    int moverandomDirection; //�����¿�������� �̵��ϴ� ����
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
    }
    

    // ���� ���� �÷��̾ ��� ���� �� ȣ��
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pattern = pattern.Chase;
            // ���� �÷��̾ ����
        }
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

                moverandomDirection = Random.Range(1, 2) == 1 ? 1 : -1;
                StartCoroutine("Move", move_time);



                break;

            case pattern.Chase:
                //float distanceX = Mathf.Abs(player.position.x - transform.position.x);

                //// X�� �������� ���� �Ÿ� �̻� ���̳� ���� �̵�
                //if (distanceX > xDistanceThreshold)
                //{
                // �÷��̾� �������� X�����θ� �̵�
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
            if (other.CompareTag("Player") && Hide == false ) // �÷��̾� �±׸� ���� ��ü ����
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
            Debug.Log("�÷��̾ ���� ������ �������ϴ�!");
        }
    }
    void Move()
    {
        for (int i = 0; i < 120; i++)
        {
            //if()// ������Ʈ ������ �ٴ��� ������ �̵� ������ �ߴ�
            //{
            //    transform.position += new Vector3(moverandomDirection * moveSpeed * Time.deltaTime, 0f, 0f);
            //}
            //else if ()
            //{

            //}


        }
        
        
    
    }
    private void Chase()
    {
        float moveDirection = Player.position.x > transform.position.x ? 1f : -1f; // �÷��̾ �������̸� 1, �����̸� -1
        transform.position += new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);
    }
}