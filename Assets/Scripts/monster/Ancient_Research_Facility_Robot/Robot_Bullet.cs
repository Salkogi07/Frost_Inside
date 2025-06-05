using UnityEngine;

public class Robot_Bullet : MonoBehaviour
{
    public float Destroy_Time = 0.1f;
    public float speed = 10f; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject,Destroy_Time );
    }

    void Update()
    {
        transform.position += new Vector3(speed * Time.deltaTime, 0f, 0f);
        // rb.velocity = direction * bulletSpeed;
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
