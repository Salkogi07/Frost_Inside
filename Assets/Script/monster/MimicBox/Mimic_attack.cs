using UnityEngine;

public class Mimic_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f,1f);
    public LayerMask groundLayer; // Ground ���̾ ����
    public GameObject pos; // OverlapBox�� �߽� ��ġ�� ������ ����



    private Monster_stat stat;
    private MimicBoxDirector boxDirector;
    float Coll;
    //public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stat = GetComponent<Monster_stat>();
        boxDirector = GetComponent<MimicBoxDirector>();
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
        Boxsize = new Vector2(stat.range[0], stat.range[1]);
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] hit = Physics2D.OverlapBoxAll(pos.transform.position, Boxsize,0f);
        foreach(Collider2D collider in hit)
        {
            if(collider.tag == "Player" && Coll <= 0f)
            {
                Coll = stat.Attack_speed;
                boxDirector.Pattern = MimicBoxDirector.pattern.Attack;
                collider.GetComponent<Player_Stats>().TakeDamage(stat.Damage);
                Debug.Log(Coll);
            }
            else
            {
                Debug.Log("�߱����Ÿ");
            }
        }
        
            
            
       
            if(Coll > 0f) Coll -= Time.deltaTime;
       }

    //public void hits()
    //{
    //    Collider2D[] hit = Physics2D.OverlapBoxAll(pos.transform.position, Boxsize, 0f);
    //    foreach (Collider2D collider in hit)
    //    {
    //        if (collider.tag == "Player" && Coll <= 0f)
    //        {
                
                
    //        }
    //    }
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos.transform.position, Boxsize); // pos�� �߽����� OverlapBox ��ġ�� ũ�⸦ �ð������� Ȯ��
    }
}
