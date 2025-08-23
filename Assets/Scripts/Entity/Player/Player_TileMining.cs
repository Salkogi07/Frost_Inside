using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class Player_TileMining : MonoBehaviour
{
    private Player player; // Player 스크립트 참조

    [Header("Mining Settings")]
    [Tooltip("채굴 대상이 될 Tilemap들을 배열로 지정하세요")]
    public Tilemap[] tilemaps;
    public float miningRange = 5f;
    public float miningTime = 2f;

    [Header("Strength Settings")]
    [Tooltip("각 타일별 방어력 및 채굴 가능 여부를 관리하는 SO")]
    public TileStrengthSettings miningSettings;

    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;
    private SpreadTilemap spreadTilemap;
    private MakeRandomMap mapGenerator;

    // 채굴 관련 상태 변수
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

    // Player_MiningState에서 매 프레임 호출될 메서드
    public void HandleMiningUpdate()
    {
        // CanMine이 false이면 로직을 중단합니다.
        if (!player.CanMine)
        {
            StopMining();
            return;
        }

        Vector3Int mouseTilePos = GetMouseTilePosition();

        var candidateMaps = tilemaps.Where(tm => tm.HasTile(mouseTilePos)).ToList();
        bool hasAnyTile = candidateMaps.Count > 0;

        if (!hasAnyTile)
        {
             StopMining();
             return;
        }

        bool inRange = candidateMaps.Any(tm =>
            Vector3.Distance(tm.GetCellCenterWorld(mouseTilePos), player.transform.position) <= miningRange);

        bool canSee = candidateMaps.Any(tm => inRange && CheckLineOfSight(tm, tm.WorldToCell(player.transform.position), mouseTilePos));
        
        if (inRange && canSee)
        {
            var map = GetTilemapAt(mouseTilePos);
            var tile = map?.GetTile(mouseTilePos);

            if (tile != null && miningSettings.IsMineable(tile))
            {
                isMining = true;
                currentMiningTile = mouseTilePos;
                UpdateMiningProgress(mouseTilePos);
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
        isMining = false;
        currentMiningTile = null;
    }
    
    // --- Helper Methods ---
    private Vector3Int GetMouseTilePosition()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return tilemaps[0].WorldToCell(mouseWorld);
    }

    private Tilemap GetTilemapAt(Vector3Int pos)
    {
        return tilemaps.FirstOrDefault(tm => tm.HasTile(pos));
    }

    private bool CheckLineOfSight(Tilemap map, Vector3Int start, Vector3Int end)
    {
        if (start == end) return true;
        int x0 = start.x, y0 = start.y;
        int x1 = end.x, y1 = end.y;
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (!(x0 == start.x && y0 == start.y) && !(x0 == x1 && y0 == y1) && map.HasTile(new Vector3Int(x0, y0, 0)))
                return false;

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
        return true;
    }
}