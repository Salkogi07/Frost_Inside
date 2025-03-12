using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Player_TileMining : MonoBehaviour
{
    [Header("Component")]
    private Player_Move player_move;
    private Player_Stats stats;

    [Header("IsMining")]
    public bool isMining = false;

    [Header("Existing settings")]
    public Tilemap tilemap;            // 실제 맵(블록이 깔린) 타일맵
    public Transform player;           // 플레이어의 위치
    public float miningRange = 5f;     // 채굴 가능 범위
    public float miningTime = 2f;      // 한 블록을 채굴하는 데 걸리는 시간

    [Header("For border emphasis")]
    public Tilemap highlightTilemap;   // 강조용 타일맵
    public Tile borderTile;            // 채굴 가능 테두리
    public Tile blockedBorderTile;     // 채굴 불가 테두리

    private Vector3Int? lastHighlightedTile = null;  // 마지막으로 강조했던 타일
    private Vector3Int? currentMiningTile = null;    // 현재 채굴 중인 타일 위치

    // 각 타일(블록)마다 현재 알파값(1=불투명, 0=완전투명)을 저장
    private Dictionary<Vector3Int, float> tileAlphaDict = new Dictionary<Vector3Int, float>();

    private void Awake()
    {
        // player가 null이라면, 이 스크립트가 달린 오브젝트의 Transform을 할당
        if (player == null)
            player = GetComponent<Transform>();

        if (player_move == null)
            player_move = GetComponent<Player_Move>();

        if (stats == null)
            stats = GetComponent<Player_Stats>();

        if (highlightTilemap == null)
            highlightTilemap = GameObject.Find("mining").GetComponent<Tilemap>();

        if (tilemap == null)
            tilemap = GameObject.Find("ground").GetComponent<Tilemap>();
    }

    private void Start()
    {
        // 초기화: 맵 전체 타일을 훑어보면서 tileAlphaDict에 alpha=1f로 등록
        BoundsInt bounds = tilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                tileAlphaDict[pos] = 1f; // 처음엔 불투명
            }
        }
    }

    private void Update()
    {
        if (stats.isDead || Inventory.instance.isInvenOpen)
            return;
        
        // 1) 마우스가 가리키는 타일 좌표 구하기
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseTilePos = tilemap.WorldToCell(mouseWorldPos);

        // 2) 범위 체크 (거리가 miningRange 이하인지)
        bool inRange = false;
        if (tilemap.HasTile(mouseTilePos))
        {
            float distance = Vector3.Distance(tilemap.GetCellCenterWorld(mouseTilePos), player.position);
            if (distance <= miningRange)
            {
                inRange = true;
            }
        }

        // 3) 만약 이전에 강조했던 타일과 현재 마우스 타일이 다르면 이전 강조 지우기
        if (lastHighlightedTile.HasValue && lastHighlightedTile.Value != mouseTilePos)
        {
            highlightTilemap.SetTile(lastHighlightedTile.Value, null);
            lastHighlightedTile = null;
        }

        // 4) 채굴 가능 여부 판단
        bool canMine = false;
        bool canSee = false;

        if (inRange)
        {
            // 플레이어 타일 좌표
            Vector3Int playerTilePos = tilemap.WorldToCell(player.position);

            // Bresenham 등으로 중간 셀이 막혔는지 검사
            canSee = CheckLineOfSight(tilemap, playerTilePos, mouseTilePos);

            // canSee가 true 면, 중간에 막는 타일이 없다고 판단
            if (canSee)
            {
                canMine = true;
            }
        }

        // 5) 테두리 표시 로직
        if (inRange)
        {
            Tile tileToUse = canSee ? borderTile : blockedBorderTile;
            highlightTilemap.SetTile(mouseTilePos, tileToUse);
            lastHighlightedTile = mouseTilePos;
        }
        else
        {
            // 범위 밖이면 테두리를 지운다
            if (lastHighlightedTile.HasValue && lastHighlightedTile.Value == mouseTilePos)
            {
                highlightTilemap.SetTile(mouseTilePos, null);
                lastHighlightedTile = null;
            }
        }

        // ---------------------------------------------
        // 채굴(마이닝) 로직
        // ---------------------------------------------
        if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Mining")))  // 마우스 왼쪽 버튼 누르고 있으면 채굴
        {
            // canMine == true 일 때만 실제 채굴 진행
            if (canMine)
            {
                isMining = true;

                // 새로 채굴 타일 선택
                if (currentMiningTile == null || currentMiningTile != mouseTilePos)
                {
                    currentMiningTile = mouseTilePos;
                }

                // 채굴 중인 타일의 알파값 가져오기
                // (Dictionary에 없으면, 아직 등록 안 된 것이므로 초기값 1f로 등록)
                if (!tileAlphaDict.ContainsKey(mouseTilePos))
                {
                    tileAlphaDict[mouseTilePos] = 1f;
                }

                float currentAlpha = tileAlphaDict[mouseTilePos];

                // 채굴 속도: 1초에 (1 / miningTime)만큼 알파가 줄어든다고 해석
                // Time.deltaTime을 곱해 실제 프레임별 감소량 계산
                float alphaDecrease = (1f / miningTime) * Time.deltaTime;
                currentAlpha -= alphaDecrease;

                // 알파값이 0 이하면 블록 제거
                if (currentAlpha <= 0f)
                {
                    // 완전히 사라짐
                    tilemap.SetTile(mouseTilePos, null);
                    tileAlphaDict.Remove(mouseTilePos);
                    currentMiningTile = null;
                    isMining = false;
                }
                else
                {
                    // 사라지진 않았으므로, 새 알파값 반영
                    tileAlphaDict[mouseTilePos] = currentAlpha;

                    // 타일에 알파값 적용
                    tilemap.SetTileFlags(mouseTilePos, TileFlags.None);
                    Color newColor = tilemap.GetColor(mouseTilePos);
                    newColor.a = currentAlpha;
                    tilemap.SetColor(mouseTilePos, newColor);
                }
            }
        }
        else
        {
            // 마우스 왼쪽 버튼을 떼면, 채굴 중이던 타일 정보를 초기화
            // (이미 깎인 알파값은 그대로 유지 -> "채굴 중단 후 재개" 가능)
            currentMiningTile = null;
            if (player_move != null)
            {
                isMining = false;
            }
        }

        // 범위 밖 블록은 알파값 원래 색 복원
        // (“범위 벗어나면 모든 블록 투명도 1로 자동 복원”이 필요한 경우)
        // 여기서는 “이미 깎인 블록은 그대로”로 유지한다고 가정하겠습니다.
        // 아래 주석 제거 시, 범위를 벗어나면 전부 알파=1로 돌아갑니다.
        /*
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                float dist = Vector3.Distance(tilemap.GetCellCenterWorld(pos), player.position);
                if (dist > miningRange)
                {
                    // 만약 "채굴 진행도도 초기화"한다면:
                    tileAlphaDict[pos] = 1f; // 알파값 1로 복원
                    tilemap.SetTileFlags(pos, TileFlags.None);
                    Color c = tilemap.GetColor(pos);
                    c.a = 1f;
                    tilemap.SetColor(pos, c);
                }
            }
        }
        */
    }

    /// <summary>
    /// Bresenham 알고리즘을 사용해 start ~ end 사이에
    /// (start와 end를 제외한) 다른 타일이 있는지 체크.
    /// true면 중간에 막힘이 없다는 의미(시야 O),
    /// false면 중간에 막힘이 있다는 의미(시야 X).
    /// </summary>
    bool CheckLineOfSight(Tilemap tilemap, Vector3Int start, Vector3Int end)
    {
        // 같은 타일이면 당연히 시야가 트여있는 것으로 처리
        if (start == end) return true;

        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = dx - dy;

        int currentX = x0;
        int currentY = y0;

        // Bresenham line
        while (true)
        {
            // 중간에 막힘 확인(시작 타일, 목표 타일은 제외)
            if (!(currentX == x0 && currentY == y0) && !(currentX == x1 && currentY == y1))
            {
                if (tilemap.HasTile(new Vector3Int(currentX, currentY, 0)))
                {
                    // 중간에 다른 타일이 있으므로 시야가 막힘
                    return false;
                }
            }

            if (currentX == x1 && currentY == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                currentX += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                currentY += sy;
            }
        }

        // 여기까지 왔다면 중간에 막힌 타일 없음
        return true;
    }
}