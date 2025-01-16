using UnityEngine;
using UnityEngine.Tilemaps;

public class Player_TileMining : MonoBehaviour
{
    public Tilemap tilemap;            // Ÿ�ϸ�
    public Transform player;           // �÷��̾��� ��ġ
    public float miningRange = 5f;     // ä�� ���� ����
    public float miningTime = 2f;      // �� ����� ä���ϴ� �� �ɸ��� �ð�
    public Color miningHighlightColor = Color.yellow; // ä�� ���� ��� ���� ����

    private Vector3Int? currentMiningTile = null; // ���� ä�� ���� Ÿ�� ��ġ
    private float miningProgress = 0f;           // ä�� ���൵

    void Update()
    {
        if (player == null)
        {
            Debug.LogError("Player Transform�� �Ҵ���� �ʾҽ��ϴ�. Inspector���� 'player' �ʵ带 �����ϼ���!");
            return;
        }

        Vector3Int playerTilePos = tilemap.WorldToCell(player.position);

        if (Input.GetMouseButton(0)) // ���콺 ���� ��ư Ŭ������ ä�� ����
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetTilePos = tilemap.WorldToCell(mouseWorldPos);

            if (Vector3.Distance(tilemap.GetCellCenterWorld(targetTilePos), player.position) <= miningRange)
            {
                if (currentMiningTile == null || currentMiningTile != targetTilePos)
                {
                    // ���ο� ����� ����
                    currentMiningTile = targetTilePos;
                    miningProgress = 0f;
                }

                if (tilemap.HasTile(targetTilePos))
                {
                    miningProgress += Time.deltaTime;

                    // ��� ����ȭ �� ���� ȿ��
                    tilemap.SetTileFlags(targetTilePos, TileFlags.None);
                    tilemap.SetColor(targetTilePos, Color.Lerp(Color.white, miningHighlightColor, miningProgress / miningTime));

                    if (miningProgress >= miningTime)
                    {
                        // ����� ����
                        tilemap.SetTile(targetTilePos, null);
                        currentMiningTile = null;
                    }
                }
            }
        }
        else
        {
            // ä�� �ߴ� �� �ʱ�ȭ
            if (currentMiningTile.HasValue)
            {
                tilemap.SetColor(currentMiningTile.Value, Color.white);
                currentMiningTile = null;
                miningProgress = 0f;
            }
        }

        // ���� ���� ����� ���� ���� ����
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (Vector3.Distance(tilemap.GetCellCenterWorld(pos), player.position) > miningRange && tilemap.HasTile(pos))
            {
                tilemap.SetTileFlags(pos, TileFlags.None);
                tilemap.SetColor(pos, Color.white);
            }
        }
    }
}
