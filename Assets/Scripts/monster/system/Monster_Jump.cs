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
    bool e = true;


    [Header("check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 size;
    public LayerMask groundLayer;
    private Rigidbody2D rb;
 
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = GameObject.FindWithTag("Mining_Tile").transform;
    }
    
    void Update()
    {
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

    public void CalculateJumpHeight()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float initialVelocity = jumpspeed;

        maxHeight = (initialVelocity * initialVelocity) / (2 * gravity);
        Debug.Log($"���� �ִ� ���� ����: {maxHeight:F2} ���� (�߷� ���)");
    }

    public void OnJump()
    {
        
        jumping = true;
        isJumping = true;
        
    }
    void FixedUpdate()
    {
        if (!jumping&&isJumping && ground_check())
        {   
            Debug.Log("??");
            isJumping = false;
        }
    }
    
    bool ground_check()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
    }
    
    //bool IsGrounded()
    //{
    //    return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    //}

    private IEnumerator Jumping()
    {
        yield return new WaitForSeconds(jump_height);
        jumping=false;
    }
}
