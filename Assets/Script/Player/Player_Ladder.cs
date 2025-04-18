using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player_Ladder : MonoBehaviour
{
    private Tilemap ladder_Tilemap;
    private Player_Stats stats;
    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Climbing")]
    [SerializeField] public float climbSpeed;
    private float climbDirection;
    public bool IsLadder = false;
    public bool IsClimbing = false;

    [Header("Detection")]
    public Transform pos;
    public Vector2 boxSize;

    [Header("Cooldown")]
    [SerializeField] public float ladderCooldownDuration = 0.5f;
    private float ladderCooldownTimer = 0f;

    private float gravityScale;
    private float tileHalfHeight;        // Ÿ�� ���� ����
    private float colliderHalfHeight;    // �÷��̾� �ݶ��̴� ���� ����

    private void Awake()
    {
        ladder_Tilemap = GameObject.FindGameObjectWithTag("Ladder").GetComponent<Tilemap>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Player_Stats>();
        col = GetComponent<Collider2D>();

        // Ÿ�� ���� ���� ��� (Grid cellSize�� y/2) :contentReference[oaicite:0]{index=0}
        tileHalfHeight = ladder_Tilemap.layoutGrid.cellSize.y * 0.5f;

        // �ݶ��̴� ���� ���� ��� (Bounds.extents.y�� ����/2) :contentReference[oaicite:1]{index=1}
        if (col != null)
            colliderHalfHeight = col.bounds.extents.y;
    }

    private void Start()
    {
        gravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (ladderCooldownTimer > 0f)
            ladderCooldownTimer -= Time.deltaTime;

        if (stats.isDead || Inventory.instance.isInvenOpen)
            return;

        LadderCheck();
        LadderOut();
        LadderActiveCheck();
        Climbing();
    }

    void LadderCheck()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(pos.position, boxSize, 0f);
        IsLadder = false;
        foreach (var c in hits)
            if (c.CompareTag("Ladder"))
                IsLadder = true;

        if (!IsLadder && IsClimbing)
            ExitLadder();
    }

    void LadderOut()
    {
        if (IsClimbing && Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Jump")))
            ExitLadder();
    }

    void ExitLadder()
    {
        IsClimbing = false;
        rb.gravityScale = gravityScale;
        rb.linearVelocity = Vector2.zero;  // ƨ�� ����
        ladderCooldownTimer = ladderCooldownDuration;
    }

    void Climbing()
    {
        if (!IsClimbing) return;

        rb.gravityScale = 0f;

        climbDirection = 0f;
        if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Ladder Move Up")))
            climbDirection = 1f;
        else if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Ladder Move Down")))
            climbDirection = -1f;

        rb.linearVelocity = new Vector2(0f, climbDirection * climbSpeed);

        AlignToLadderTopIfNeeded();
    }

    void AlignToLadderTopIfNeeded()
    {
        Vector3 playerPos = transform.position;
        BoundsInt bounds = ladder_Tilemap.cellBounds;

        float minXDiff = float.MaxValue;
        int topCellY = int.MinValue;
        Vector3Int topCell = Vector3Int.zero;

        foreach (var cell in bounds.allPositionsWithin)
        {
            if (!ladder_Tilemap.HasTile(cell)) continue;
            Vector3 center = ladder_Tilemap.GetCellCenterWorld(cell);
            float dx = Mathf.Abs(playerPos.x - center.x);

            if (dx < minXDiff || (Mathf.Approximately(dx, minXDiff) && cell.y > topCellY))
            {
                minXDiff = dx;
                topCellY = cell.y;
                topCell = cell;
            }
        }

        if (minXDiff < Mathf.Infinity)
        {
            Vector3 topWorld = ladder_Tilemap.GetCellCenterWorld(topCell);
            // �� ��� Y ��ǥ
            float footTargetY = topWorld.y + tileHalfHeight;
            // �߹ٴ� ����: �÷��̾� �߽� Y = ��ǥ �߹ٴ� Y + �ݶ��̴� �ݳ���
            float centerTargetY = footTargetY + colliderHalfHeight;

            if (playerPos.y >= centerTargetY)
            {
                transform.position = new Vector3(playerPos.x, centerTargetY, playerPos.z);
                ExitLadder();
            }
        }
    }

    void LadderActiveCheck()
    {
        if (ladderCooldownTimer > 0f || IsClimbing) return;

        if (IsLadder && Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Interaction")))
        {
            Vector3 p = transform.position;
            BoundsInt b = ladder_Tilemap.cellBounds;
            float minDX = float.MaxValue;
            float targetX = p.x;

            foreach (var cell in b.allPositionsWithin)
            {
                if (!ladder_Tilemap.HasTile(cell)) continue;
                Vector3 c = ladder_Tilemap.GetCellCenterWorld(cell);
                float dx = Mathf.Abs(p.x - c.x);
                if (dx < minDX)
                {
                    minDX = dx;
                    targetX = c.x;
                }
            }

            transform.position = new Vector3(targetX, p.y, p.z);
            IsClimbing = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (pos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(pos.position, boxSize);
        }
    }
}
