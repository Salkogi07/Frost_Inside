using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player_Ladder : MonoBehaviour
{
    private Tilemap ladder_Tilemap;
    private Tilemap ladderTop_Tilemap;
    private Player_Stats stats;
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
    [SerializeField] public float ladderCooldownDuration = 0.5f; // ��Ÿ�� ���ӽð� (��)
    private float ladderCooldownTimer = 0f; // ���� ��Ÿ��

    private float gravityScale = 0f;

    private void Awake()
    {
        ladder_Tilemap = GameObject.FindGameObjectWithTag("Ladder").GetComponent<Tilemap>();
        //ladderTop_Tilemap = GameObject.FindGameObjectWithTag("LadderTop").GetComponent<Tilemap>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Player_Stats>();
    }

    private void Start()
    {
        if(rb != null)
            gravityScale = rb.gravityScale;
    }

    private void Update()
    {
        // ��Ÿ�� Ÿ�̸� ������Ʈ
        if (ladderCooldownTimer > 0)
            ladderCooldownTimer -= Time.deltaTime;

        if(stats.isDead || Inventory.instance.isInvenOpen)
            return;

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

            // ��ٸ� ������ ���µ� Ŭ���̹� ���¶��, �ڵ����� Ŭ���̹� ��� ����
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
        // ��ٸ� Ż�� �� ��Ÿ�� ����
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
        // ��Ÿ���� ���� ���̸� ��ٸ� Ȱ��ȭ ���� �� ��
        if (ladderCooldownTimer > 0)
            return;

        if (IsLadder && !IsClimbing)
        {
            if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Interaction")))
            {
                if (ladder_Tilemap == null)
                {
                    Debug.LogError("Ladder Tilemap�� �Ҵ���� �ʾҽ��ϴ�!");
                    return;
                }

                Vector3 playerPos = transform.position;
                float minXDistance = Mathf.Infinity;
                float targetX = playerPos.x;

                // Ÿ�ϸ��� ��ü �� ������ ��ȸ�ϸ� ���� Ÿ���� �ִ� �� �� ���� ����� x ��ǥ ã��
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

                // x ��ǥ�� Ÿ���� �߾����� ���߰� y, z ��ǥ�� �״�� ����
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
