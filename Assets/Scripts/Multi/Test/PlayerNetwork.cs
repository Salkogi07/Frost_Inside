// --- START OF MODIFIED FILE PlayerNetwork.cs ---

using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// NetworkTransform이 물리 동기화를 처리하므로 Rigidbody2D가 필요합니다.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkTransform))] // NetworkTransform이 필수임을 명시
public class PlayerNetwork : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    // 점프 애니메이션 등을 위한 NetworkVariable. 위치 동기화에는 사용하지 않습니다.
    private NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody2D rb;
    private Animator animator; // 애니메이터가 있다면 연결

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 이 스크립트는 오직 소유자(Owner)의 클라이언트에서만 활성화되도록 합니다.
        // 다른 클라이언트에서는 이 스크립트의 Update()가 실행될 필요가 없습니다.
        // NetworkTransform이 알아서 위치를 동기화해줄 것이기 때문입니다.
        if (!IsOwner)
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        // IsOwner 체크가 필수적이지만, OnNetworkSpawn에서 이미 비활성화했으므로 이중으로 안전합니다.
        if (!IsOwner) return;

        // --- 입력 처리 ---
        float moveX = Input.GetAxisRaw("Horizontal");
        
        // --- 물리 처리 (FixedUpdate로 옮기는 것이 더 정확합니다) ---
        // 아래 로직을 FixedUpdate로 옮기겠습니다.

        // --- 점프 처리 ---
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping.Value = true; // 점프 상태를 네트워크로 전파 (애니메이션 등에 사용)
        }
        else if (isGrounded)
        {
            isJumping.Value = false;
        }

        // 애니메이터 업데이트 (예시)
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveX));
            animator.SetBool("IsJumping", isJumping.Value);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // 물리 관련 코드는 FixedUpdate에서 처리하는 것이 좋습니다.
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // 캐릭터 방향 전환 (필요 시)
        if (moveX > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveX < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}