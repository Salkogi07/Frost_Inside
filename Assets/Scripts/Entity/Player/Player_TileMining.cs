using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class Player_TileMining : MonoBehaviour
{
    private Player player;

    [Header("Mining Settings")]
    public Tilemap[] tilemaps;
    public float miningRange = 5f;
    public float miningTime = 2f;
    [SerializeField] private LayerMask miningLayerMask;

    [Header("Strength Settings")]
    public TileStrengthSettings miningSettings;

    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;
    private SpreadTilemap spreadTilemap;
    private MakeRandomMap mapGenerator;
    
    // --- 디버깅 옵션 ---
    [Header("Debug")]
    [Tooltip("활성화하면 콘솔에 상세한 채굴 과정을 출력합니다.")]
    public bool enableDebugLogs = true;
    // ---

    private Dictionary<Vector3Int, float> tileAlphaDict = new Dictionary<Vector3Int, float>();
    private Vector3Int? currentMiningTile = null;
    public bool isMining { get; private set; } = false;

    private void Awake()
    {
        player = GetComponent<Player>();
        spreadTilemap = FindObjectOfType<SpreadTilemap>();
        mapGenerator = FindObjectOfType<MakeRandomMap>();

        if (tilemaps == null || tilemaps.Length == 0)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("Mining_Tile");
            tilemaps = new Tilemap[gos.Length];
            for (int i = 0; i < gos.Length; i++)
            {
                tilemaps[i] = gos[i].GetComponent<Tilemap>();
            }
        }
    }

    private void Start()
    {
        foreach (var map in tilemaps)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
                if (map.HasTile(pos))
                    tileAlphaDict[pos] = 1f;
        }
    }

    public Vector3Int? GetCurrentMiningTile()
    {
        return currentMiningTile;
    }

    public void HandleMiningAndLaserUpdate()
    {
        var mousePos = (Vector2)player.Laser.cam.ScreenToWorldPoint(Input.mousePosition);
        var firePointPos = (Vector2)player.Laser.firePoint.position;
        Vector2 direction = (mousePos - firePointPos).normalized;

        RaycastHit2D hit = Physics2D.Raycast(firePointPos, direction, miningRange, miningLayerMask);

        // --- 디버깅 코드 시작 ---
        // Scene 뷰에서 레이저 경로를 시각적으로 표시합니다.
        // 초록색: 충돌 O, 빨간색: 충돌 X
        if (enableDebugLogs)
        {
            Color rayColor = hit.collider != null ? Color.green : Color.red;
            Debug.DrawRay(firePointPos, direction * miningRange, rayColor);
        }
        // --- 디버깅 코드 끝 ---

        Vector2 endPoint = (hit.collider != null) ? hit.point : firePointPos + direction * miningRange;
        player.Laser.UpdateLaser(endPoint);

        if (!player.CanMine)
        {
            StopMining();
            return;
        }

        if (hit.collider != null)
        {
            // --- 디버깅 코드 시작 ---
            if (enableDebugLogs)
                Debug.Log($"[채굴] 레이저 충돌: {hit.collider.name}, 레이어: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            // --- 디버깅 코드 끝 ---

            Vector3Int tilePos = tilemaps[0].WorldToCell(hit.point);
            var map = GetTilemapAt(tilePos);
            var tile = map?.GetTile(tilePos);

            if (tile != null && miningSettings.IsMineable(tile))
            {
                if (currentMiningTile.HasValue && currentMiningTile.Value != tilePos)
                {
                    StopMining();
                }

                isMining = true;
                currentMiningTile = tilePos;
                UpdateMiningProgress(tilePos);
            }
            else
            {
                if (enableDebugLogs && tile == null) Debug.LogWarning($"[채굴] 충돌 지점에 타일이 없습니다. 좌표: {tilePos}");
                else if (enableDebugLogs && tile != null && !miningSettings.IsMineable(tile)) Debug.LogWarning($"[채굴] '{tile.name}' 타일은 채굴 불가능합니다.");
                
                StopMining();
            }
        }
        else
        {
            if (enableDebugLogs) Debug.Log("[채굴] 레이저가 아무것에도 닿지 않음. 채굴 중지.");
            StopMining();
        }
    }

    private void UpdateMiningProgress(Vector3Int tilePos)
    {
        if (!tileAlphaDict.ContainsKey(tilePos))
            tileAlphaDict[tilePos] = 1f;

        var map = GetTilemapAt(tilePos);
        var tileBase = map.GetTile(tilePos);

        float defense = miningSettings.GetDefense(tileBase);
        float miningPower = player.Stats.Mining.GetValue();
        float timeToMine = miningTime * (defense / Mathf.Max(miningPower, 1f));
        float decrease = (timeToMine > 0) ? Time.deltaTime / timeToMine : 1.0f;

        tileAlphaDict[tilePos] -= decrease;
        
        // --- 디버깅 코드 시작 ---
        if (enableDebugLogs)
            Debug.Log($"[채굴] 타일 [{tilePos}] 채굴 진행... 남은 내구도: {tileAlphaDict[tilePos]:F2}");
        // --- 디버깅 코드 끝 ---
        
        ApplyTileAlpha(tilePos, Mathf.Clamp01(tileAlphaDict[tilePos]));

        if (tileAlphaDict[tilePos] <= 0f)
            FinishMining(tilePos);
    }
    
    private void ApplyTileAlpha(Vector3Int tilePos, float alpha)
    {
        var map = GetTilemapAt(tilePos);
        if (map == null) return;

        map.SetTileFlags(tilePos, TileFlags.None);
        Color c = map.GetColor(tilePos);
        c.a = Mathf.Clamp01(alpha);
        map.SetColor(tilePos, c);
    }

    private void FinishMining(Vector3Int tilePos)
    {
        // --- 디버깅 코드 시작 ---
        if (enableDebugLogs)
            Debug.Log($"[채굴] 타일 [{tilePos}] 채굴 완료!");
        // --- 디버깅 코드 끝 ---
        
        Vector3 worldPos = spreadTilemap.OreTilemap.CellToWorld(tilePos) + new Vector3(0.5f, 0.5f);
        Vector2Int key = new Vector2Int(tilePos.x, tilePos.y);

        if (mapGenerator.oreTileDict.TryGetValue(key, out var ore))
        {
            var dropObj = Instantiate(dropPrefab, worldPos + Vector3.up * 0.5f, Quaternion.identity);
            var itemObject = dropObj.GetComponent<ItemObject>();
            itemObject.SetupItem(ore.dropItem, Vector2.zero);
            
            Inventory_Item data = itemObject.item;
            int price = Random.Range(data.data.priceRange.x, data.data.priceRange.y + 1);
            data.price = price;
            mapGenerator.oreTileDict.Remove(key);
        }

        var map = GetTilemapAt(tilePos);
        if (map != null)
        {
             map.SetTile(tilePos, null);
        }
       
        spreadTilemap.OreTilemap.RefreshTile(tilePos);
        tileAlphaDict.Remove(tilePos);
        StopMining();
    }

    public void StopMining()
    {
        if (isMining && enableDebugLogs)
             Debug.Log("[채굴] StopMining() 호출됨. 현재 타겟: " + (currentMiningTile.HasValue ? currentMiningTile.Value.ToString() : "없음"));

        isMining = false;
        currentMiningTile = null;
    }
    
    private Tilemap GetTilemapAt(Vector3Int pos)
    {
        return tilemaps.FirstOrDefault(tm => tm.HasTile(pos));
    }
}