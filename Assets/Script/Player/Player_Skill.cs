using System.Collections;
using UnityEngine;

public class Player_Skill : MonoBehaviour
{
    Player_Move playerMove;
    Rigidbody2D rb;
    TrailRenderer tr;

    [Header("Dash info")]
    [SerializeField] public bool canDash = true;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    private float dashDir;
    float originalGravity;

    void Awake()
    {
        playerMove = GetComponent<Player_Move>();
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            playerMove.isDashing = true;
            canDash = false;
            tr.emitting = true;
            originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;

            dashDir = playerMove.isFacingRight ? 1 : -1;

            dashDir = Input.GetAxisRaw("Horizontal");

            StartCoroutine(StopDashing());
        }

        if(playerMove.isDashing)
        {
            rb.linearVelocityX = dashDir * dashingPower;
            rb.linearVelocityY = 0;
        }
    }

    private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        playerMove.isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
