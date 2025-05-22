using UnityEngine;

public class HillDetection : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(1.0f, 1.0f); // OverlapBox ũ��
    public LayerMask groundLayer; // Ground ���̾ ����
    public GameObject pos; // OverlapBox�� �߽� ��ġ�� ������ ����
    public bool Operation;

    public bool Groundcheck = false;


    private Monster_Jump monster_Jump;
    //private Monster   �����ʿ�

    private void Start()
    {
        //if(monster_Jump != null)
        //{
            monster_Jump = GetComponent<Monster_Jump>();
        //}
        

    }
    void Update()
    {
        //CheckForHillAhead();

        //if()
        //{


        //}
    }

    public void CheckForHillAhead()
    {
        // pos ������ ������ ��ġ�� OverlapBox�� ������ Ground ���̾��� ������Ʈ�� ����
        Collider2D hit = Physics2D.OverlapBox(pos.transform.position, boxSize, 0f, groundLayer);
        Debug.Log(hit);
        if (hit != null && monster_Jump.jump_cooltime <= 0f)
        {
            // Ground ���̾��� ������Ʈ�� �߰��� ���
            Groundcheck = true;
            
            checkon();
            Operation = true;
               
//(hit.tag == "Ground" || hit.tag == "Mining_Tile") &&
            // ��: ���Ͱ� �����ϵ��� ���
            

            }
        else
        {
            Groundcheck = false;
            Operation = false;
        }
    }


    private void checkon()
    {
        
        
                
                monster_Jump.CalculateJumpHeight();
                Vector3 originalPosition = pos.transform.position;
                float MaxHeightpercent = monster_Jump.maxHeight - (monster_Jump.maxHeight * 80 / 100);
                for (float i = 0f; i < monster_Jump.maxHeight; i += MaxHeightpercent)
                    {
            Collider2D hit = Physics2D.OverlapBox(pos.transform.position, boxSize, 0f, groundLayer);
            if (hit != null)
                        {

                             Debug.Log(monster_Jump.maxHeight);
                        pos.transform.position += new Vector3(0f, MaxHeightpercent, 0f);
                        }
                        else
                        {
                            Debug.Log("��");
                           monster_Jump.OnJump(); // Jump()�� Monster_Jump Ŭ���� ���� �Լ���� ����
                           monster_Jump.jump_cooltime = 5f;
                           break;
                        }
                        
                    }
                    Debug.Log("������");
                pos.transform.position = originalPosition;
                Operation = false;




        // ���⼭ ����� ���̳� ���¸� üũ�Ͽ�, �÷��̾ �ö� �� �ִ��� Ȯ���մϴ�.
        // ���� ���, hit�� Collider2D�� ���� �߰����� ������ �ۼ��� �� �ֽ��ϴ�.
    }
    // ����׸� ���� �ð��� �ڽ� �׸���
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.transform.position, boxSize); // pos�� �߽����� OverlapBox ��ġ�� ũ�⸦ �ð������� Ȯ��
    }
}
