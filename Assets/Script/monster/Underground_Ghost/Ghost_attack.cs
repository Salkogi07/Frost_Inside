using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Ghost_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f, 1f);
    public LayerMask groundLayer; // Ground 레이어를 설정
    float stay;
    public float Maxstay = 1f;

    private Monster_stat stat;

    public Player_Stats playerstat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stat = GetComponent<Monster_stat>();

        Boxsize = new Vector2(stat.range[0], stat.range[1]);
    }

    // 수정필요
    void Update()
    {
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, Boxsize, 0f,groundLayer);
        foreach (Collider2D collider in hit)
        {
            if (collider.tag == "Player")
            {
                stay += Time.deltaTime;
                

                Debug.Log(collider);
                Debug.Log(stay);
                if (stay >= Maxstay)
                {
                    //collider.GetComponent<Player_Stats>().TakeDamage(10);
                    playerstat.stamina -= 10;
                    stay = 0f;
                    Debug.Log("1");
                }
                
                
            }
            else
            {
                Debug.Log(stay);
                stay = 0f;
            }
        }
    }


    //원모양으로 바꿔야하는데 ㅈㄴ귀찮아서 임시로
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Boxsize); // pos를 중심으로 OverlapBox 위치와 크기를 시각적으로 확인
    }
}
