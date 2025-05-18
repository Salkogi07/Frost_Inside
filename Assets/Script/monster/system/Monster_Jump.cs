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
    [SerializeField] public float maxHeight;

    private Rigidbody2D rb;
 
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
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
        
    }
    private IEnumerator Jumping()
    {
        yield return new WaitForSeconds(jump_height);
        jumping=false;
    }
}
