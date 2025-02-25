using UnityEngine;
using UnityEngine.Tilemaps;

public class Player_Ladder : MonoBehaviour
{
    private Tilemap ladder_Tilemap;
    private Tilemap ladderTop_Tilemap;
    private Rigidbody2D rb;

    [Header("Climing")]
    [SerializeField] public float climbSpeed;
    [SerializeField] private float climbDirection;
    public bool IsLadder = false;
    public bool IsClimbing = false;

    [Header("Layer")]
    public Transform pos;
    public Vector2 boxSize;

    [Header("Cooldown")]
    [SerializeField] public float ladderCooldownDuration = 0.5f; // 쿨타임 지속시간 (초)
    private float ladderCooldownTimer = 0f; // 남은 쿨타임

    private float gravityScale = 0f;

    private void Awake()
    {
        ladder_Tilemap = GameObject.FindGameObjectWithTag("Ladder").GetComponent<Tilemap>();
        //ladderTop_Tilemap = GameObject.FindGameObjectWithTag("LadderTop").GetComponent<Tilemap>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if(rb != null)
            gravityScale = rb.gravityScale;
    }

    private void Update()
    {
        // 쿨타임 타이머 업데이트
        if (ladderCooldownTimer > 0)
            ladderCooldownTimer -= Time.deltaTime;

        LadderCheck();
        LadderOut();
        LadderActiveCheck();
        Climbing();
    }

    void LadderCheck()
    {
        if (pos != null)
        {
            bool ladderFound = false;
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.gameObject.CompareTag("Ladder"))
                {
                    ladderFound = true;
                    break;
                }
            }
            IsLadder = ladderFound;

            // 사다리 영역에 없는데 클라이밍 상태라면, 자동으로 클라이밍 모드 종료
            if (!IsLadder && IsClimbing)
            {
                LadderExit();
            }
        }
    }

    void LadderOut()
    {
        if (IsClimbing)
        {
            if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Jump")))
            {
                LadderExit();
            }
        }
    }

    void LadderExit()
    {
        IsClimbing = false;
        rb.gravityScale = gravityScale;
        // 사다리 탈출 시 쿨타임 시작
        ladderCooldownTimer = ladderCooldownDuration;
    }

    void Climbing()
    {
        climbDirection = 0;

        if (IsClimbing)
        {
            rb.gravityScale = 0;

            if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Move Down")))
            {
                climbDirection = -1;
            }
            if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Move Up")))
            {
                climbDirection = 1;
            }

            rb.linearVelocity = new Vector2(0, climbDirection * climbSpeed);
        }
    }

    void LadderActiveCheck()
    {
        // 쿨타임이 진행 중이면 사다리 활성화 실행 안 함
        if (ladderCooldownTimer > 0)
            return;

        if (IsLadder && !IsClimbing)
        {
            if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Interaction")))
            {
                if (ladder_Tilemap == null)
                {
                    Debug.LogError("Ladder Tilemap이 할당되지 않았습니다!");
                    return;
                }

                Vector3 playerPos = transform.position;
                float minXDistance = Mathf.Infinity;
                float targetX = playerPos.x;

                // 타일맵의 전체 셀 범위를 순회하며 실제 타일이 있는 셀 중 가장 가까운 x 좌표 찾기
                BoundsInt bounds = ladder_Tilemap.cellBounds;
                foreach (Vector3Int cellPos in bounds.allPositionsWithin)
                {
                    if (ladder_Tilemap.HasTile(cellPos))
                    {
                        Vector3 cellCenter = ladder_Tilemap.GetCellCenterWorld(cellPos);
                        float xDistance = Mathf.Abs(playerPos.x - cellCenter.x);
                        if (xDistance < minXDistance)
                        {
                            minXDistance = xDistance;
                            targetX = cellCenter.x;
                        }
                    }
                }

                // x 좌표만 타일의 중앙으로 맞추고 y, z 좌표는 그대로 유지
                transform.position = new Vector3(targetX, playerPos.y, playerPos.z);
                IsClimbing = true;
            }
        }
    }


    private void OnDrawGizmos()
    {
        if(pos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(pos.position, boxSize);
        }   
    }
}
