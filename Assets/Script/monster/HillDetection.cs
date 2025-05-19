using UnityEngine;

public class HillDetection : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(1.0f, 1.0f); // OverlapBox 크기
    public LayerMask groundLayer; // Ground 레이어를 설정
    public GameObject pos; // OverlapBox의 중심 위치를 지정할 변수

    public bool Groundcheck = false;


    private Monster_Jump monster_Jump;
    //private Monster   수정필요

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
        // pos 변수로 지정된 위치에 OverlapBox를 던져서 Ground 레이어의 오브젝트를 감지
        Collider2D[] hit = Physics2D.OverlapBoxAll(pos.transform.position, boxSize, 0f, groundLayer);
        
        if (hit != null)
        {
            // Ground 레이어의 오브젝트를 발견한 경우
            Groundcheck = true;

            //    Debug.Log("앞에 언덕이 있습니다.");

            // 예: 몬스터가 점프하도록 명령
            if (monster_Jump.jump_cooltime <= 0f)  //수정필요
            {

                monster_Jump.CalculateJumpHeight();
                Vector3 originalPosition = pos.transform.position;
                float MaxHeightpercent = monster_Jump.maxHeight - (monster_Jump.maxHeight * 80 / 100);
                for (float i = 0f; i < monster_Jump.maxHeight; i += MaxHeightpercent)
                    {

                        
                    pos.transform.position += new Vector3(0f,monster_Jump.maxHeight,0f);
                    foreach (Collider2D collider in hit)
                    {
                        if (collider.tag != "Ground" || collider.tag != "Mining_Tile")
                        {
                            Debug.Log("노");
                           monster_Jump.OnJump(); // Jump()는 Monster_Jump 클래스 내의 함수라고 가정
                           monster_Jump.jump_cooltime = 5f;
                           break;
                        }
                        //Debug.Log("연결됬노");
                    }

                }
                pos.transform.position = originalPosition;

            }

                // 여기서 언덕의 높이나 형태를 체크하여, 플레이어가 올라갈 수 있는지 확인합니다.
                // 예를 들어, hit의 Collider2D에 따라 추가적인 로직을 작성할 수 있습니다.
            }
        else
        {
            Groundcheck = false;
        }
    }

    // 디버그를 위한 시각적 박스 그리기
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.transform.position, boxSize); // pos를 중심으로 OverlapBox 위치와 크기를 시각적으로 확인
    }
}
