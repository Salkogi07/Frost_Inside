using System.Collections;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float jumpForce = 10f;

    [SerializeField] public float coyoteTime = 0.2f;
    [SerializeField] public float jumpBufferTime = 0.2f;

    private float gravityScale = 3.5f;

    [Header("Ground Check")]
    [SerializeField] private bool isGrounded;
    [SerializeField] public float groundCheckDistance;
    [SerializeField] public Transform groundCheck;
    [SerializeField] public Vector2 groundCheckSize = new Vector2(1f, 0.1f);
    [SerializeField] public LayerMask groundLayer;

    [Header("Platform Check")]
    [SerializeField] public LayerMask platformLayer;
    [SerializeField] PlatformEffector2D effector;
    [SerializeField] public bool isPlatform = false;

    [Header("Component")]
    public ParticleSystem dust;
    private SpriteRenderer spriteRenderer;
    public Rigidbody2D rb { get; private set; }
    private Player_Skill playerSkill;

    [Header("IsAtcitoning")]
    [SerializeField] private bool isJumping;
    [SerializeField] public bool isFacingRight = false;
    bool isreversed;

    private int facingDir;
    private int moveInput = 0;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    public bool isDashing = false;
    public bool isAttack = false;
    private bool isJumpCut = false;

    [Header("Double Jump")]
    [SerializeField] private bool canDoubleJump = true;
    private bool doubleJumpAvailable = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSkill = GetComponent<Player_Skill>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        gravityScale = rb.gravityScale;
    }

    void Update()
    {
        if (isDashing)
        {
            return;
        }

        PlatformCheck();
        GroundCheck();

        MoveInput();
        CheckJumpPlatform();

        Flip();
    }


    void MoveInput()
    {
        moveInput = 0;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveInput = -1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveInput = 1;
        }
        rb.linearVelocity = new Vector2(moveInput * moveSpeed / (isAttack ? 2 : 1), rb.linearVelocity.y);
    }

    private void Flip()
    {
        if (isAttack)
            return;

        if (isFacingRight && moveInput < 0f || !isFacingRight && moveInput > 0f)
        {
            if (isGrounded)
                CreateDust();

            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public void FlipAttack(int directionX)
    {
        if (isFacingRight && directionX < 0 || !isFacingRight && directionX > 0)
        {
            if (isGrounded)
                CreateDust();

            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }


    private void Jump()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            doubleJumpAvailable = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            if(jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping)
        {
            CreateDust();
            PerformJump();
            isJumpCut = true;
        }
        else if (canDoubleJump && doubleJumpAvailable && !isGrounded && Input.GetButtonDown("Jump"))
        {
            PerformJump();
            isJumpCut = true;
            doubleJumpAvailable = false;
        }

        if (isJumpCut && Input.GetButtonUp("Jump") && rb.linearVelocityY > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
            coyoteTimeCounter = 0f;
            isJumpCut = false;
        }
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
        jumpBufferCounter = 0f;
        StartCoroutine(JumpCooldown());
    }

    private IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
    }

    private void CheckJumpPlatform()
    {
        if (isPlatform)
        {
            if (Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetButtonDown("Jump"))
                    ReverseOneWay();
            }
            else
            {
                Jump();
            }
        }
        else
        {
            Jump();
        }
    }

    private void PlatformCheck()
    {
        Collider2D collider = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, platformLayer);
        isPlatform = collider != null;
    }

    private void GroundCheck()
    {
        Collider2D collider = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        isGrounded = collider != null;
    }

    void CreateDust()
    {
        dust.Play();
    }

    /*private void AnimationController()
    {
        if (isAttack)
        {
            animator.PlayAnimation("Attack");
        }
        else if (!isGrounded && rb.linearVelocityY < 0)
        {
            animator.PlayAnimation("Fall");
        }
        else if (!isGrounded && rb.linearVelocityY > 0)
        {
            animator.PlayAnimation("Jump");
        }
        else if (isGrounded && moveInput != 0)
        {
            animator.PlayAnimation("Move");
        }
        else if (isGrounded && moveInput == 0)
        {
            animator.PlayAnimation("Idle");
        }
    }*/

    IEnumerator _reversePlatformEffector()
    {
        effector.rotationalOffset = 180f;
        yield return new WaitForSeconds(.5f);
        effector.rotationalOffset = 0f;
        isreversed = false;
    }

    public void ReverseOneWay()
    {
        if (isreversed) return;
        isreversed = true;
        StartCoroutine(_reversePlatformEffector());
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
