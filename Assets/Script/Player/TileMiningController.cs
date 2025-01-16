using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMiningController : MonoBehaviour
{
    public Tilemap tilemap;            // 타일맵
    public Transform player;           // 플레이어의 위치
    public float miningRange = 5f;     // 채굴 가능 범위
    public float miningTime = 2f;      // 한 블록을 채굴하는 데 걸리는 시간
    public Color miningHighlightColor = Color.yellow; // 채굴 중인 블록 강조 색상

    private Vector3Int? currentMiningTile = null; // 현재 채굴 중인 타일 위치
    private float miningProgress = 0f;           // 채굴 진행도

    void Update()
    {
        if (player == null)
        {
            Debug.LogError("Player Transform이 할당되지 않았습니다. Inspector에서 'player' 필드를 설정하세요!");
            return;
        }

        Vector3Int playerTilePos = tilemap.WorldToCell(player.position);

        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼 클릭으로 채굴 시작
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetTilePos = tilemap.WorldToCell(mouseWorldPos);

            if (Vector3.Distance(tilemap.GetCellCenterWorld(targetTilePos), player.position) <= miningRange)
            {
                if (currentMiningTile == null || currentMiningTile != targetTilePos)
                {
                    // 새로운 블록을 선택
                    currentMiningTile = targetTilePos;
                    miningProgress = 0f;
                }

                if (tilemap.HasTile(targetTilePos))
                {
                    miningProgress += Time.deltaTime;

                    // 블록 투명화 및 강조 효과
                    tilemap.SetTileFlags(targetTilePos, TileFlags.None);
                    tilemap.SetColor(targetTilePos, Color.Lerp(Color.white, miningHighlightColor, miningProgress / miningTime));

                    if (miningProgress >= miningTime)
                    {
                        // 블록을 제거
                        tilemap.SetTile(targetTilePos, null);
                        currentMiningTile = null;
                    }
                }
            }
        }
        else
        {
            // 채굴 중단 시 초기화
            if (currentMiningTile.HasValue)
            {
                tilemap.SetColor(currentMiningTile.Value, Color.white);
                currentMiningTile = null;
                miningProgress = 0f;
            }
        }

        // 범위 밖의 블록은 원래 색상 복원
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
