using UnityEngine;

public class Robot_Bullet : MonoBehaviour
{
    public float Destroy_Time;
    public float speed; 
    
    private Transform player;
    private Rigidbody2D rb;
    Vector3 direction;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject,Destroy_Time );
        player = GameObject.FindGameObjectWithTag("Player").transform;
         direction = (player.position - transform.position).normalized;
         // speed += Random.Range();
         float _Y_random = Random.Range(-0.7f, 0.7f);
         transform.position += new Vector3(0f, _Y_random, 0f);
    }

    void Update()
    {
        // transform.position += new Vector3(speed * Time.deltaTime, 0f, 0f);
        rb.linearVelocity = direction * speed;
    }
    
    // IEnumerator attackingAnimationToEnd()
    // {
    //     //animator.SetTrigger("Attack");
    //     
    //     //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    //     
    //     //yield return new WaitForSeconds(stateInfo.length);
    //     yield return new WaitForSeconds(1f);
    //     
    //     Debug.Log("빙칠링!");
    // }
}
