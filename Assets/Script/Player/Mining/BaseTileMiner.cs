using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseTileMiner : MonoBehaviour
{
    [Header("Component")]
    protected Player_Move playerMove;
    protected Player_Stats stats;

    [Header("Mining Settings")]
    // 단일 Tilemap → 여러 Tilemap 배열로 변경
    [Tooltip("채굴 대상이 될 Tilemap들을 배열로 지정하세요")]
    public Tilemap[] tilemaps;
    public Transform player;
    public float miningRange = 5f;
    public float miningTime = 2f;

    [Header("Strength Settings")]
    [Tooltip("각 타일별 방어력 및 채굴 가능 여부를 관리하는 SO")]
    public TileStrengthSettings miningSettings;   // ← 추가

    [Header("Highlight Settings")]
    public Tilemap highlightTilemap;
    public Tile borderTile;
    public Tile blockedBorderTile;

    protected Vector3Int? lastHighlightedTile = null;
    protected Vector3Int? currentMiningTile = null;

    // 각 타일별 현재 알파값(1=불투명, 0=완전투명)
    protected Dictionary<Vector3Int, float> tileAlphaDict = new Dictionary<Vector3Int, float>();

    // 채굴 중 여부
    public bool isMining = false;

    protected virtual void Awake()
    {
        playerMove = GetComponent<Player_Move>();
        stats = GetComponent<Player_Stats>();
        if (player == null) player = transform;

        if (tilemaps == null || tilemaps.Length == 0)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Mining_Tile");
            tilemaps = new Tilemap[gos.Length];
            for (int i = 0; i < gos.Length; i++)
            {
                tilemaps[i] = gos[i].GetComponent<Tilemap>();
            }
        }


        if (highlightTilemap == null)
            highlightTilemap = GameObject.Find("mining_Check").GetComponent<Tilemap>();
    }

    protected virtual void Start()
    {
        // 여러 Tilemap을 순회하며 알파값 초기화
        foreach (var map in tilemaps)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
                if (map.HasTile(pos))
                    tileAlphaDict[pos] = 1f;
        }
    }

    protected virtual void Update()
    {
        if (!GameManager.instance.isSetting)
            return;

        if (!CanUpdate()) return;

        Vector3Int mouseTilePos = GetMouseTilePosition();

        // 여러 Tilemap 중 하나라도 타일이 있으면 채굴 대상
        bool hasAnyTile = tilemaps.Any(tm => tm.HasTile(mouseTilePos));
        // 범위 체크 (첫 번째 맵 좌표 사용)
        bool inRange = hasAnyTile &&
            Vector3.Distance(
                tilemaps[0].GetCellCenterWorld(mouseTilePos),
                player.position
            ) <= miningRange;
        // 시야 체크 (첫 번째 맵 좌표 사용)
        bool canSee = inRange && CheckLineOfSight(
            tilemaps[0],
            tilemaps[0].WorldToCell(player.position),
            mouseTilePos
        );

        UpdateHighlight(mouseTilePos, inRange, canSee);
        HandleMining(mouseTilePos, inRange, canSee);
    }

    protected virtual bool CanUpdate()
        => !stats.isDead && !Inventory.instance.isInvenOpen;

    protected virtual Vector3Int GetMouseTilePosition()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return tilemaps[0].WorldToCell(mouseWorld);
    }

    protected virtual void UpdateHighlight(Vector3Int tilePos, bool inRange, bool canSee)
    {
        // 1) 이전 강조 해제
        if (lastHighlightedTile.HasValue && lastHighlightedTile.Value != tilePos)
        {
            highlightTilemap.SetTile(lastHighlightedTile.Value, null);
            lastHighlightedTile = null;
        }

        // 2) 현재 타일 위 & 범위 내일 때만 강조
        if (hasAnyTileAt(tilePos) && inRange)
        {
            Tile toUse = canSee ? borderTile : blockedBorderTile;
            highlightTilemap.SetTile(tilePos, toUse);
            lastHighlightedTile = tilePos;
        }
    }

    protected virtual void HandleMining(Vector3Int tilePos, bool inRange, bool canSee)
    {
        if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Mining"))
        && inRange && canSee)
        {
            var map = GetTilemapAt(tilePos);
            var tile = map?.GetTile(tilePos);
            // ▶ 채굴 불가 타일이면 즉시 종료
            if (tile == null || !miningSettings.IsMineable(tile))
            {
                // TODO: 불가 이펙트/사운드 재생
                StopMining();
                return;
            }

            isMining = true;
            currentMiningTile = tilePos;
            UpdateMining(tilePos);
        }
        else
        {
            StopMining();
        }
    }

    protected virtual void UpdateMining(Vector3Int tilePos)
    {
        if (!tileAlphaDict.ContainsKey(tilePos))
            tileAlphaDict[tilePos] = 1f;

        float decrease = (1f / miningTime) * Time.deltaTime;
        tileAlphaDict[tilePos] -= decrease;

        ApplyTileAlpha(tilePos, tileAlphaDict[tilePos]);

        if (tileAlphaDict[tilePos] <= 0f)
            FinishMining(tilePos);
    }

    protected virtual void ApplyTileAlpha(Vector3Int tilePos, float alpha)
    {
        // 해당 좌표에 타일이 있는 Tilemap을 찾아 적용
        var map = GetTilemapAt(tilePos);
        if (map == null) return;

        map.SetTileFlags(tilePos, TileFlags.None);
        Color c = map.GetColor(tilePos);
        c.a = Mathf.Clamp01(alpha);
        map.SetColor(tilePos, c);
    }

    protected virtual void FinishMining(Vector3Int tilePos)
    {
        var map = GetTilemapAt(tilePos);
        if (map != null)
            map.SetTile(tilePos, null);

        // ↓ 추가: 강조 하이라이트 타일 제거
        highlightTilemap.SetTile(tilePos, null);
        lastHighlightedTile = null;

        tileAlphaDict.Remove(tilePos);
        currentMiningTile = null;
        isMining = false;
    }

    protected virtual void StopMining()
    {
        isMining = false;
        currentMiningTile = null;
    }

    /// <summary>
    /// 여러 Tilemap에서 해당 좌표의 타일이 있는지 확인
    /// </summary>
    protected bool hasAnyTileAt(Vector3Int pos)
        => tilemaps.Any(tm => tm.HasTile(pos));

    /// <summary>
    /// 여러 Tilemap 중 해당 좌표에 타일이 있는 첫 번째 Tilemap 반환
    /// </summary>
    protected Tilemap GetTilemapAt(Vector3Int pos)
    {
        foreach (var tm in tilemaps)
            if (tm.HasTile(pos))
                return tm;
        return null;
    }

    /// <summary>
    /// Bresenham 알고리즘으로 시야 검사
    /// </summary>
    protected bool CheckLineOfSight(Tilemap map, Vector3Int start, Vector3Int end)
    {
        if (start == end) return true;
        int x0 = start.x, y0 = start.y;
        int x1 = end.x, y1 = end.y;
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (!(x0 == start.x && y0 == start.y)
             && !(x0 == x1 && y0 == y1)
             && map.HasTile(new Vector3Int(x0, y0, 0)))
                return false;

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
        return true;
    }
}
