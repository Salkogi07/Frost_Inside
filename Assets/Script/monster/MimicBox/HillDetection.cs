using UnityEngine;

public class HillDetection : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(1.0f, 1.0f); // OverlapBox ũ��
    public LayerMask groundLayer; // Ground ���̾ ����
    public GameObject pos; // OverlapBox�� �߽� ��ġ�� ������ ����

    public bool Groundcheck = false;


    private Monster_Jump monster_Jump;
    //private monster

    private void Start()
    {
        monster_Jump = GetComponent<Monster_Jump>();

    }
    void Update()
    {   
        CheckForHillAhead();

        if()
        {
            for (int i = 0; i < 10; i++)
            {

            }

        }
    }

    void CheckForHillAhead()
    {
        // pos ������ ������ ��ġ�� OverlapBox�� ������ Ground ���̾��� ������Ʈ�� ����
        Collider2D hit = Physics2D.OverlapBox(pos.transform.position, boxSize, 0f, groundLayer);

        if (hit != null)
        {
            // Ground ���̾��� ������Ʈ�� �߰��� ���
            Debug.Log("�տ� ����� �ֽ��ϴ�.");
            Groundcheck = true;


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
