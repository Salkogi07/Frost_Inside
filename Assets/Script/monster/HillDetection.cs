using UnityEngine;

public class HillDetection : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(1.0f, 1.0f); // OverlapBox ũ��
    public LayerMask groundLayer; // Ground ���̾ ����
    public GameObject pos; // OverlapBox�� �߽� ��ġ�� ������ ����

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
        
        if (hit != null)
        {
            // Ground ���̾��� ������Ʈ�� �߰��� ���
            Groundcheck = true;

            //    Debug.Log("�տ� ����� �ֽ��ϴ�.");

            // ��: ���Ͱ� �����ϵ��� ���
            if (monster_Jump.jump_cooltime <= 0f)  //�����ʿ�
            {

                monster_Jump.CalculateJumpHeight();
                Vector3 originalPosition = pos.transform.position;
                float MaxHeightpercent = monster_Jump.maxHeight - (monster_Jump.maxHeight * 80 / 100);
                for (float i = 0f; i < monster_Jump.maxHeight; i += MaxHeightpercent)
                    {

                        
                    pos.transform.position += new Vector3(0f,monster_Jump.maxHeight,0f);
                    if (hit == null)
                    {
                        monster_Jump.OnJump(); // Jump()�� Monster_Jump Ŭ���� ���� �Լ���� ����
                        monster_Jump.jump_cooltime = 5f;
                        break;
                       
                    }
                 
                
                    }
                pos.transform.position = originalPosition;

            }

                // ���⼭ ����� ���̳� ���¸� üũ�Ͽ�, �÷��̾ �ö� �� �ִ��� Ȯ���մϴ�.
                // ���� ���, hit�� Collider2D�� ���� �߰����� ������ �ۼ��� �� �ֽ��ϴ�.
            }
        else
        {
            Groundcheck = false;
        }
    }

    // ����׸� ���� �ð��� �ڽ� �׸���
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.transform.position, boxSize); // pos�� �߽����� OverlapBox ��ġ�� ũ�⸦ �ð������� Ȯ��
    }
}
