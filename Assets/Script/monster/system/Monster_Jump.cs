using System.Collections;
using UnityEngine;


public class Monster_Jump : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("jump_stat")]
    [SerializeField] public float jumpspeed = 0.1f;
    [SerializeField] public float jump_cooltime;
    [SerializeField] public float jump_height = 0.4f;
    [SerializeField] bool jumping;


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
            Debug.Log("예상 점프 y 이동량: " + expectedJumpY.ToString("F2"));
            StartCoroutine(Jumping());
        }
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
