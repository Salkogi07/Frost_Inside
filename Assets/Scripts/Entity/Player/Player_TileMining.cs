using UnityEngine;
using UnityEngine.Tilemaps;

public class Player_TileMining : MonoBehaviour
{ 
    private Player player;

    [Header("Mining Settings")]
    [SerializeField] private float miningRange = 5f; // 레이저의 기본 사거리
    [SerializeField] private LayerMask miningLayerMask; // 채굴 가능한 오브젝트들의 레이어 마스크

    [Header("Strength Settings")]
    [SerializeField] private TileStrengthSettings miningSettings; // 타일 강도 설정 ScriptableObject

    // 현재 채굴 중인 타겟에 대한 정보
    private Tilemap currentMiningTilemap;      // 현재 채굴 중인 타일맵
    private Vector3Int currentMiningTilePosition; // 현재 채굴 중인 타일의 위치
    private TileBase currentMiningTileBase;       // 현재 채굴 중인 타일의 정보
    
    private float currentMiningProgress = 0f; // 현재 채굴 진행도

    private void Awake()
    {
        if (player == null)
            player = GetComponent<Player>();
    }

    /// <summary>
    /// Player_MiningState에서 매 프레임 호출되어 채굴 로직과 레이저 업데이트를 처리합니다.
    /// </summary>
    public void HandleMiningAndLaserUpdate()
    {
        Vector2 firePointPos = player.Laser.firePoint.position;
        Vector2 mouseWorldPos = player.Laser.cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - firePointPos).normalized;

        RaycastHit2D hit = Physics2D.Raycast(firePointPos, direction, miningRange, miningLayerMask);
        
        if (hit.collider != null)
        {
            Debug.Log("Hit" + hit.collider.name);
            // 1. 레이캐스트에 감지된 오브젝트에서 Tilemap 컴포넌트를 가져옵니다.
            Tilemap hitTilemap = hit.collider.GetComponent<Tilemap>();
            // 2. Tilemap 컴포넌트가 존재하는지 확인합니다.
            if (hitTilemap != null)
            {
                Debug.Log("HitTile" + hitTilemap.name);
                // 레이저 끝점을 맞은 위치로 업데이트
                player.Laser.UpdateLaser(hit.point);

                Vector2 hitPoint = hit.point - hit.normal * 0.01f;
                Vector3Int tilePosition = hitTilemap.WorldToCell(hitPoint);
                Debug.Log("tilePosition:" + tilePosition);
                
                TileBase tile = hitTilemap.GetTile(tilePosition);
                
                
                Debug.Log("tile:" + tile);

                // 3. 해당 타일이 존재하고, 채굴 가능한 타일인지 확인합니다.
                if (tile != null && miningSettings.IsMineable(tile))
                {
                    // 이전에 캐던 타일과 다른 타일을 조준한 경우, 진행도를 초기화합니다.
                    // 또는 다른 타일맵을 조준한 경우에도 초기화합니다.
                    if (tilePosition != currentMiningTilePosition || hitTilemap != currentMiningTilemap)
                    {
                        StopMining();
                        currentMiningTilemap = hitTilemap; // 새로 감지된 타일맵을 현재 타겟으로 설정
                        currentMiningTilePosition = tilePosition;
                        currentMiningTileBase = tile;
                    }
                    
                    // 채굴 진행
                    UpdateMiningProgress();
                }
                else
                {
                    // 채굴 불가능한 타일(기반암 등)이거나 빈 공간을 조준한 경우
                    StopMining();
                }
            }
            else
            {
                // miningLayerMask에 있지만 Tilemap 컴포넌트가 없는 오브젝트에 맞은 경우
                StopMining();
                player.Laser.UpdateLaser(hit.point);
            }
        }
        else
        {
            // 레이저가 아무것에도 닿지 않았을 때
            StopMining();
            player.Laser.UpdateLaser(firePointPos + direction * miningRange); // 기본 사거리로 레이저 표시
        }
    }

    /// <summary>
    /// 채굴 진행도를 업데이트하고, 완료되면 타일을 파괴합니다.
    /// </summary>
    private void UpdateMiningProgress()
    {
        if (currentMiningTilemap == null || currentMiningTileBase == null) return;

        float miningStatValue = Mathf.Max(1, player.Stats.Mining.GetValue());
        float tileDefense = miningSettings.GetDefense(currentMiningTileBase);
        float timeToMine = tileDefense / miningStatValue;

        currentMiningProgress += Time.deltaTime;

        if (currentMiningProgress >= timeToMine)
        {
            // 현재 채굴 중인 타일맵에서 해당 타일을 제거합니다.
            currentMiningTilemap.SetTile(currentMiningTilePosition, null);
            StopMining();
        }
    }

    /// <summary>
    /// 채굴을 멈추고 관련 변수들을 초기화합니다.
    /// </summary>
    public void StopMining()
    {
        currentMiningProgress = 0f;
        currentMiningTilemap = null;
        currentMiningTileBase = null;
    }
}