using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Player_TileMining : MonoBehaviour
{
    [Header("���� ����")]
    public Tilemap tilemap;            // ���� ��(����� ��) Ÿ�ϸ�
    public Transform player;           // �÷��̾��� ��ġ
    public float miningRange = 5f;     // ä�� ���� ����
    public float miningTime = 2f;      // �� ����� ä���ϴ� �� �ɸ��� �ð�

    [Header("�׵θ� ������")]
    public Tilemap highlightTilemap;   // ������ Ÿ�ϸ�
    public Tile borderTile;            // ä�� ���� �׵θ�
    public Tile blockedBorderTile;     // ä�� �Ұ� �׵θ�

    private Vector3Int? lastHighlightedTile = null;  // ���������� �����ߴ� Ÿ��
    private Vector3Int? currentMiningTile = null;    // ���� ä�� ���� Ÿ�� ��ġ

    // �� Ÿ��(���)���� ���� ���İ�(1=������, 0=��������)�� ����
    private Dictionary<Vector3Int, float> tileAlphaDict = new Dictionary<Vector3Int, float>();

    private void Awake()
    {
        // player�� null�̶��, �� ��ũ��Ʈ�� �޸� ������Ʈ�� Transform�� �Ҵ�
        if (player == null)
            player = GetComponent<Transform>();

        if (highlightTilemap == null)
            highlightTilemap = GameObject.Find("mining").GetComponent<Tilemap>();

        if (tilemap == null)
            tilemap = GameObject.Find("ground").GetComponent<Tilemap>();
    }

    private void Start()
    {
        // highlightTilemap�� ���� ������ ���� Ÿ�ϸʺ��� ������ ����
        TilemapRenderer renderer = highlightTilemap.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = -2;  // �� Ÿ�ϸʺ��� ���� ��
        }

        // �ʱ�ȭ: �� ��ü Ÿ���� �Ⱦ�鼭 tileAlphaDict�� alpha=1f�� ���
        BoundsInt bounds = tilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                tileAlphaDict[pos] = 1f; // ó���� ������
            }
        }
    }

    void Update()
    {
        // 1) ���콺�� ����Ű�� Ÿ�� ��ǥ ���ϱ�
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseTilePos = tilemap.WorldToCell(mouseWorldPos);

        // 2) ���� üũ (�Ÿ��� miningRange ��������)
        bool inRange = false;
        if (tilemap.HasTile(mouseTilePos))
        {
            float distance = Vector3.Distance(tilemap.GetCellCenterWorld(mouseTilePos), player.position);
            if (distance <= miningRange)
            {
                inRange = true;
            }
        }

        // 3) ���� ������ �����ߴ� Ÿ�ϰ� ���� ���콺 Ÿ���� �ٸ��� ���� ���� �����
        if (lastHighlightedTile.HasValue && lastHighlightedTile.Value != mouseTilePos)
        {
            highlightTilemap.SetTile(lastHighlightedTile.Value, null);
            lastHighlightedTile = null;
        }

        // 4) ä�� ���� ���� �Ǵ�
        bool canMine = false;
        bool canSee = false;

        if (inRange)
        {
            // �÷��̾� Ÿ�� ��ǥ
            Vector3Int playerTilePos = tilemap.WorldToCell(player.position);

            // Bresenham ������ �߰� ���� �������� �˻�
            canSee = CheckLineOfSight(tilemap, playerTilePos, mouseTilePos);

            // canSee�� true ��, �߰��� ���� Ÿ���� ���ٰ� �Ǵ�
            if (canSee)
            {
                canMine = true;
            }
        }

        // 5) �׵θ� ǥ�� ����
        if (inRange)
        {
            Tile tileToUse = canSee ? borderTile : blockedBorderTile;
            highlightTilemap.SetTile(mouseTilePos, tileToUse);
            lastHighlightedTile = mouseTilePos;
        }
        else
        {
            // ���� ���̸� �׵θ��� �����
            if (lastHighlightedTile.HasValue && lastHighlightedTile.Value == mouseTilePos)
            {
                highlightTilemap.SetTile(mouseTilePos, null);
                lastHighlightedTile = null;
            }
        }

        // ---------------------------------------------
        // ä��(���̴�) ����
        // ---------------------------------------------
        if (Input.GetMouseButton(0))  // ���콺 ���� ��ư ������ ������ ä��
        {
            // canMine == true �� ���� ���� ä�� ����
            if (canMine)
            {
                // ���� ä�� Ÿ�� ����
                if (currentMiningTile == null || currentMiningTile != mouseTilePos)
                {
                    currentMiningTile = mouseTilePos;
                }

                // ä�� ���� Ÿ���� ���İ� ��������
                // (Dictionary�� ������, ���� ��� �� �� ���̹Ƿ� �ʱⰪ 1f�� ���)
                if (!tileAlphaDict.ContainsKey(mouseTilePos))
                {
                    tileAlphaDict[mouseTilePos] = 1f;
                }

                float currentAlpha = tileAlphaDict[mouseTilePos];

                // ä�� �ӵ�: 1�ʿ� (1 / miningTime)��ŭ ���İ� �پ��ٰ� �ؼ�
                // Time.deltaTime�� ���� ���� �����Ӻ� ���ҷ� ���
                float alphaDecrease = (1f / miningTime) * Time.deltaTime;
                currentAlpha -= alphaDecrease;

                // ���İ��� 0 ���ϸ� ��� ����
                if (currentAlpha <= 0f)
                {
                    // ������ �����
                    tilemap.SetTile(mouseTilePos, null);
                    tileAlphaDict.Remove(mouseTilePos);
                    currentMiningTile = null;
                }
                else
                {
                    // ������� �ʾ����Ƿ�, �� ���İ� �ݿ�
                    tileAlphaDict[mouseTilePos] = currentAlpha;

                    // Ÿ�Ͽ� ���İ� ����
                    tilemap.SetTileFlags(mouseTilePos, TileFlags.None);
                    Color newColor = tilemap.GetColor(mouseTilePos);
                    newColor.a = currentAlpha;
                    tilemap.SetColor(mouseTilePos, newColor);
                }
            }
        }
        else
        {
            // ���콺 ���� ��ư�� ����, ä�� ���̴� Ÿ�� ������ �ʱ�ȭ
            // (�̹� ���� ���İ��� �״�� ���� -> "ä�� �ߴ� �� �簳" ����)
            currentMiningTile = null;
        }

        // ���� �� ����� ���İ� ���� �� ����
        // (������ ����� ��� ��� ���� 1�� �ڵ� �������� �ʿ��� ���)
        // ���⼭�� ���̹� ���� ����� �״�Ρ��� �����Ѵٰ� �����ϰڽ��ϴ�.
        // �Ʒ� �ּ� ���� ��, ������ ����� ���� ����=1�� ���ư��ϴ�.
        /*
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                float dist = Vector3.Distance(tilemap.GetCellCenterWorld(pos), player.position);
                if (dist > miningRange)
                {
                    // ���� "ä�� ���൵�� �ʱ�ȭ"�Ѵٸ�:
                    tileAlphaDict[pos] = 1f; // ���İ� 1�� ����
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
    /// Bresenham �˰����� ����� start ~ end ���̿�
    /// (start�� end�� ������) �ٸ� Ÿ���� �ִ��� üũ.
    /// true�� �߰��� ������ ���ٴ� �ǹ�(�þ� O),
    /// false�� �߰��� ������ �ִٴ� �ǹ�(�þ� X).
    /// </summary>
    bool CheckLineOfSight(Tilemap tilemap, Vector3Int start, Vector3Int end)
    {
        // ���� Ÿ���̸� �翬�� �þ߰� Ʈ���ִ� ������ ó��
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
            // �߰��� ���� Ȯ��(���� Ÿ��, ��ǥ Ÿ���� ����)
            if (!(currentX == x0 && currentY == y0) && !(currentX == x1 && currentY == y1))
            {
                if (tilemap.HasTile(new Vector3Int(currentX, currentY, 0)))
                {
                    // �߰��� �ٸ� Ÿ���� �����Ƿ� �þ߰� ����
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

        // ������� �Դٸ� �߰��� ���� Ÿ�� ����
        return true;
    }
}
