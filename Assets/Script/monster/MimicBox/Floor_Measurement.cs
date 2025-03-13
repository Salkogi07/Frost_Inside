using UnityEngine;
using UnityEngine.EventSystems;

public class Floor_Measurement : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(1.0f, 1.0f); // OverlapBox ũ��
    public LayerMask groundLayer; // Ground ���̾ ����
    public GameObject pos; // OverlapBox�� �߽� ��ġ�� ������ ����
    private GameObject MimicBox;
    public Rigidbody2D rb { get; private set; }

    public bool Groundcheck = false;
    public float direction = 1f;

    void Update()
    {

        CheckForHillAhead();
        Change_of_location();
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
    void Change_of_location()
    {
        //Vector3 newPosition = MimicBox.transform.position;
        //if(direction == 1f)
        //{


        //    // X ��ǥ�� speed�� ����ؼ� �̵���ŵ�ϴ�.
        //    newPosition.x *= 1f;
        //}
        //else
        //{
        //    newPosition.x *= -1f;
        //}
        //transform.position = newPosition;
    }


    // ����׸� ���� �ð��� �ڽ� �׸���
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos.transform.position, boxSize); // pos�� �߽����� OverlapBox ��ġ�� ũ�⸦ �ð������� Ȯ��
    }
}
