using System.Collections;
using UnityEngine;


public class Monster_Jump : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("jump_stat")]
    [SerializeField] public float jumpspeed = 0.1f;
    [SerializeField] public float jump_cooltime = 0f;
    [SerializeField] public float jump_height = 0.4f;
    [SerializeField] public bool jumping;
    [SerializeField] public bool isJumping;
    [SerializeField] public float maxHeight;
    [SerializeField] public float savedY;
    bool e = true;

    public LayerMask groundLayer;
    private Transform groundCheck;
    private Rigidbody2D rb;
 
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = GameObject.FindWithTag("Mining_Tile").transform;
        //groundCheck = TileMining.GetComponent<Transform>();

    }

    // Update is called once per frame
    void Update()
    {
        if(e)
        {
            e = !e;
            //groundCheck = GameObject.FindWithTag("Mining_Tile").transform;
            groundCheck = GameObject.FindWithTag("Mining_Tile").transform;
            Debug.Log("yuy");
        }
        if (jump_cooltime >0)
        {
            jump_cooltime -= Time.deltaTime;
        }
        if (jumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpspeed);

            float expectedJumpY = jumpspeed * jump_height;
            //CalculateJumpHeight();
            StartCoroutine(Jumping());
        }
    }
//Debug.Log("���� ���� y �̵���: " + expectedJumpY.ToString("F2"));
    public void CalculateJumpHeight()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale); // ���� �߷�
        float initialVelocity = jumpspeed; // ���� ���� �ӵ�

        maxHeight = (initialVelocity * initialVelocity) / (2 * gravity);
        Debug.Log($"���� �ִ� ���� ����: {maxHeight:F2} ���� (�߷� ���)");
    }

    public void OnJump()
    {
        Debug.Log("??");
        jumping = true;
        isJumping = true;
        
    }
    void FixedUpdate()
    {
        // �����ߴ��� Ȯ���ؼ� ���� ���� ����
        if (isJumping && IsGrounded())
        {
            isJumping = false;
        }
    }
    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private IEnumerator Jumping()
    {
        yield return new WaitForSeconds(jump_height);
        jumping=false;
    }
}
