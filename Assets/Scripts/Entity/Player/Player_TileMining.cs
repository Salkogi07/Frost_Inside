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
        Vector2 firePointPos = player.Laser.rotationPoint.position;
        Vector2 mouseWorldPos = player.Laser.cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - firePointPos).normalized;
        
        player.CheckAndFlip(direction.x);
        player.SetMiningAnimationByDirection(direction);

        RaycastHit2D hit = Physics2D.Raycast(firePointPos, direction, miningRange, miningLayerMask);
        
        Vector2 laserEndPoint; // 레이저 끝점을 저장할 변수

        if (hit.collider != null)
        {
            laserEndPoint = hit.point; // 레이저 끝점을 맞은 위치로 설정
            
            Tilemap hitTilemap = hit.collider.GetComponent<Tilemap>();
            if (hitTilemap != null)
            {
                Vector2 hitPoint = hit.point - hit.normal * 0.01f;
                Vector3Int tilePosition = hitTilemap.WorldToCell(hitPoint);
                TileBase tile = hitTilemap.GetTile(tilePosition);

                if (tile != null && miningSettings.GetIsMineable(tile))
                {
                    if (tilePosition != currentMiningTilePosition || hitTilemap != currentMiningTilemap)
                    {
                        StopMining();
                        currentMiningTilemap = hitTilemap;
                        currentMiningTilePosition = tilePosition;
                        currentMiningTileBase = tile;
                    }
                    UpdateMiningProgress();
                }
                else
                {
                    StopMining();
                }
            }
            else
            {
                StopMining();
            }
        }
        else
        {
            laserEndPoint = firePointPos + direction * miningRange; // 기본 사거리로 레이저 끝점 설정
            StopMining();
        }
        
        // Owner 클라이언트에서만 레이저를 직접 업데이트하고, 네트워크 변수 갱신
        if (player.IsOwner)
        {
            // 1. 로컬 레이저 시각적 요소 업데이트
            player.Laser.UpdateLaser(laserEndPoint); 
            
            // 2. 네트워크를 통해 다른 클라이언트에 레이저 상태 전송
            player.UpdateLaserState(true, laserEndPoint, player.Laser.transform.rotation);
        }
    }

    /// <summary>
    /// 채굴 진행도를 업데이트하고, 완료되면 타일을 파괴합니다.
    /// </summary>
    private void UpdateMiningProgress()
    {
        if (currentMiningTilemap == null || currentMiningTileBase == null) return;

        float miningStatValue = Mathf.Max(1, player.Stats.Mining.GetValue());
        bool tileIsMine = miningSettings.GetIsMineable(currentMiningTileBase);
        float tileDefense = miningSettings.GetDefense(currentMiningTileBase);
        
        if (tileDefense == -1 || !tileIsMine)
        {
            StopMining();
            return;
        }
        
        float timeToMine = tileDefense / miningStatValue;

        currentMiningProgress += Time.deltaTime;

        if (currentMiningProgress >= timeToMine)
        {
            // 현재 채굴 중인 타일맵에서 해당 타일을 제거합니다.
            if (MapInteractionManager.Instance != null)
            {
                Debug.Log($"[Mining] Requesting to mine tile at {currentMiningTilePosition}");
                TileMapType tileMapType = MapInteractionManager.Instance.spreadTilemap.GetTileMapType(currentMiningTilemap);
                MapInteractionManager.Instance.RequestMineTileServerRpc(currentMiningTilePosition, tileMapType);
            }
            else
            {
                Debug.LogError("MapInteractionManager instance not found! Cannot send mining request.");
            }
            
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