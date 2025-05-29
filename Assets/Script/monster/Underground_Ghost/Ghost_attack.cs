using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Ghost_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f, 1f);
    public LayerMask groundLayer; // Ground ���̾ ����
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

    // �����ʿ�
    void Update()
    {

        Collider2D hit = Physics2D.OverlapBox(transform.position, Boxsize, 0f, groundLayer);

        if (hit != null)
        {
            stay += Time.deltaTime;
            if (stay >= Maxstay)
            {
                //�µ� ����
                
                playerstat.temperature -= 1.25f;
                stay = 0f;
               
            }
        }
        else
        {
            stay = 0f;
             
        }


    }


    //��������� �ٲ���ϴµ� ���������Ƽ� �ӽ÷�
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Boxsize); // pos�� �߽����� OverlapBox ��ġ�� ũ�⸦ �ð������� Ȯ��
    }
}
