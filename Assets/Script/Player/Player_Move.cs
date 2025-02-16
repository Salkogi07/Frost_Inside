using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    [Header("Component")]
    public ParticleSystem dust;
    private SpriteRenderer spriteRenderer;
    private PlayerStats stats;

    public Rigidbody2D rb { get; private set; }

    [Header("Player Info")]
    [SerializeField] public float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float currentSpeed;
    [SerializeField] private bool isSprinting = false;

    [Header("Stamina info")]
    [SerializeField] private float sprintCost = 5f; // 초당 스테미나 감소량
    [SerializeField] private float jumpCost = 25f; // 초당 스테미나 감소량
    [SerializeField] private float staminaRecoverRate = 15f; // 초당 스테미나 회복량
    [SerializeField] private float staminaDecreaseRate = 10f; // 초당 스테미나 갑소량

    [Header("Stamina Cooldown")]
    [SerializeField] private float staminaCooldownDuration = 2f;
    private float staminaCooldownTimer = 0f;

    [Header("Temperature Info")]
    [SerializeField] private float temperatureDropRate = 1f;
    [SerializeField] private float hpDropRate = 2f;
    private float damageTimer = 0f;
    [SerializeField] private bool isNearHeatSource = false; // 불 근처에 있는지 체크
    [SerializeField] private bool isInColdZone = false; // 추운 지역인지 체크

    [Header("Jump Info")]
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


    [Header("IsMining")]
    public bool isMining = false;

    [Header("IsAtcitoning")]
    [SerializeField] private bool isJumping;
    [SerializeField] public bool isFacingRight = false;
    bool isreversed;

    private int facingDir;
    private int moveInput = 0;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    public bool isAttack = false;
    private bool isJumpCut = false;

    [Header("Double Jump")]
    [SerializeField] private bool canDoubleJump = true;
    private bool doubleJumpAvailable = false;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        stats = GetComponent<PlayerStats>();

        gravityScale = rb.gravityScale;
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        if (isMining)
        {
            return;
        }

        Sprint();
        HandleStamina();
        HandleTemperature();
        HandleHp();

        PlatformCheck();
        GroundCheck();

        MoveInput();
        CheckJumpPlatform();

        Flip();

    }

    void Sprint()
    {
        // Shift를 누르고 있고, 스테미나가 남아있을 경우 달리기
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
            currentSpeed = sprintSpeed;
        }
        else
        {
            isSprinting = false;
            currentSpeed = walkSpeed;
        }
    }

    void MoveInput()
    {
        moveInput = 0;

        if (Input.GetKey(KeyCode.A))
        {
            moveInput = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveInput = 1;
        }
        rb.linearVelocity = new Vector2(moveInput * currentSpeed / (isAttack ? 2 : 1), rb.linearVelocity.y);
    }

    void HandleStamina()
    {
        float tempRatio = stats.GetTemperature();
        int tempState = Temp(tempRatio);

        if (isSprinting)
        {
            if (stats.stamina > sprintCost * Time.deltaTime)
            {
                stats.stamina -= sprintCost * Time.deltaTime;
            }
            else
            {
                stats.stamina = 0;
                isSprinting = false;
                currentSpeed = walkSpeed;
                staminaCooldownTimer = staminaCooldownDuration;
            }
        }
        else
        {
            if (staminaCooldownTimer > 0)
            {
                staminaCooldownTimer -= Time.deltaTime;
            }
            else if (stats.stamina <= stats.maxStamina)
            {
                if (tempState == 3)
                {
                    if (stats.stamina > 0)
                    {
                        stats.stamina -= staminaDecreaseRate * Time.deltaTime;
                        if (stats.stamina < 0)
                            stats.stamina = 0;
                    }
                }
                else if (tempState == 2)
                {
                    float targetStamina = stats.maxStamina * 0.5f;

                    // 스태미나가 목표치보다 높다면 서서히 감소
                    if (stats.stamina > targetStamina)
                    {
                        stats.stamina -= staminaDecreaseRate * Time.deltaTime;
                        if (stats.stamina < targetStamina)
                            stats.stamina = targetStamina;
                    }
                    else
                    {
                        // 목표치보다 낮으면 회복 (최대 targetStamina까지만 회복)
                        stats.stamina += staminaRecoverRate * Time.deltaTime;
                        if (stats.stamina > targetStamina)
                            stats.stamina = targetStamina;
                    }
                }
                else
                {
                    stats.stamina += staminaRecoverRate * Time.deltaTime;
                    if (stats.stamina > stats.maxStamina)
                    {
                        stats.stamina = stats.maxStamina;
                    }
                }
            }
        }
    }

    void HandleHp()
    {
        float tempRatio = stats.GetTemperature();

        if (tempRatio == 0)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= 1f)
            {
                stats.hp -= hpDropRate;
                damageTimer = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    private static int Temp(float tempRatio)
    {
        int tempState;
        if (tempRatio >= 0.75f)
        {
            tempState = 0; // 정상 체온
        }
        else if (tempRatio >= 0.5f)
        {
            tempState = 1; // 약간 추움
        }
        else if (tempRatio >= 0.25f)
        {
            tempState = 2; // 많이 추움
        }
        else
        {
            tempState = 3; // 극도로 추움
        }

        return tempState;
    }

    void HandleTemperature()
    {
        float dropRate = temperatureDropRate;

        if (moveInput != 0)
            dropRate *= 0.5f; // 움직이면 감소율 줄이기

        if (isSprinting)
            dropRate *= 0.25f; // 뛰고 있으면 온도 감소율 더 낮추기

        if (isAttack)
            dropRate *= 0.5f; // 공격 중에는 감소율 반으로

        stats.temperature -= dropRate * Time.deltaTime;
        stats.temperature = Mathf.Max(stats.temperature, 0f);
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
            if (jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping)
        {
            if (stats.stamina >= jumpCost)
            {
                stats.stamina -= jumpCost;
                CreateDust();
                PerformJump();
                isJumpCut = true;
            }
        }
        else if (canDoubleJump && doubleJumpAvailable && !isGrounded && Input.GetButtonDown("Jump"))
        {
            if (stats.stamina >= jumpCost)
            {
                stats.stamina -= jumpCost;
                PerformJump();
                isJumpCut = true;
                doubleJumpAvailable = false;
            }
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
