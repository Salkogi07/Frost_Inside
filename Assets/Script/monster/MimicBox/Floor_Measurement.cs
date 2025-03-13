using UnityEngine;
using UnityEngine.EventSystems;

public class Floor_Measurement : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(1.0f, 1.0f); // OverlapBox 크기
    public LayerMask groundLayer; // Ground 레이어를 설정
    public GameObject pos; // OverlapBox의 중심 위치를 지정할 변수
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
        // pos 변수로 지정된 위치에 OverlapBox를 던져서 Ground 레이어의 오브젝트를 감지
        Collider2D hit = Physics2D.OverlapBox(pos.transform.position, boxSize, 0f, groundLayer);

        if (hit != null)
        {
            // Ground 레이어의 오브젝트를 발견한 경우
            Debug.Log("앞에 언덕이 있습니다.");
            Groundcheck = true;


            // 여기서 언덕의 높이나 형태를 체크하여, 플레이어가 올라갈 수 있는지 확인합니다.
            // 예를 들어, hit의 Collider2D에 따라 추가적인 로직을 작성할 수 있습니다.
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


        //    // X 좌표만 speed에 비례해서 이동시킵니다.
        //    newPosition.x *= 1f;
        //}
        //else
        //{
        //    newPosition.x *= -1f;
        //}
        //transform.position = newPosition;
    }


    // 디버그를 위한 시각적 박스 그리기
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos.transform.position, boxSize); // pos를 중심으로 OverlapBox 위치와 크기를 시각적으로 확인
    }
}
