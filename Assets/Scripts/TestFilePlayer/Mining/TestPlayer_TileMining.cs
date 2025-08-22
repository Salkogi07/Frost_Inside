using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TestPlayer_TileMining : BaseTileMiner
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;         // 드롭할 프리팹
    private SpreadTilemap spreadTilemap;    // Ore 타일맵 참조
    private MakeRandomMap mapGenerator;    // oreTileDict 참조

    [Header("Tool Settings")]
    private float _toolPower = 10f;         // 플레이어의 채굴력

    protected override void Awake()
    {
        spreadTilemap = FindObjectOfType<SpreadTilemap>();
        mapGenerator = FindObjectOfType<MakeRandomMap>();

        base.Awake();
        // 추가 초기화 예: 사운드 매니저, 이펙트 준비 등
    }

    protected override void UpdateMining(Vector3Int tilePos)
    {
        if (!tileAlphaDict.ContainsKey(tilePos))
            tileAlphaDict[tilePos] = 1f;

        var map = GetTilemapAt(tilePos);
        var tileBase = map.GetTile(tilePos);

        // ▶ 방어력 조회
        float defense = miningSettings.GetDefense(tileBase);
        // ▶ 채굴 시간 계산 = miningTime × (defense ÷ toolPower)
        float timeToMine = miningTime * (defense / Mathf.Max(_toolPower, 0.0001f));
        // ▶ 한 프레임당 알파 감소량 = Time.deltaTime ÷ timeToMine
        float decrease = Time.deltaTime / timeToMine;

        tileAlphaDict[tilePos] -= decrease;
        ApplyTileAlpha(tilePos, Mathf.Clamp01(tileAlphaDict[tilePos]));

        if (tileAlphaDict[tilePos] <= 0f)
            FinishMining(tilePos);

        // TODO: 채굴 이펙트·사운드 재생
    }

    protected override void FinishMining(Vector3Int tilePos)
    {
        // 1) 셀 → 월드 좌표 변환
        Vector3 worldPos = spreadTilemap.OreTilemap
                            .CellToWorld(tilePos) + new Vector3(0.5f, 0.5f);
        Vector2Int key = new Vector2Int(tilePos.x, tilePos.y);

        // 2) oreTileDict에 정보가 있으면 아이템 드롭
        if (mapGenerator.oreTileDict.TryGetValue(key, out var ore))
        {
            var dropObj = Instantiate(
                dropPrefab,
                worldPos + Vector3.up * 0.5f,
                Quaternion.identity
            );

            dropObj.GetComponent<ItemObject>()
                   .SetupItem(ore.dropItem, Vector2.zero);

            Inventory_Item data = dropObj.GetComponent<ItemObject>().item;
            int price = Random.Range(data.data.priceRange.x, data.data.priceRange.y + 1);
            data.price = price;
            mapGenerator.oreTileDict.Remove(key);
        }

        // 3) OreTilemap에서 타일 제거
        spreadTilemap.OreTilemap.SetTile(tilePos, null);
        spreadTilemap.OreTilemap.RefreshTile(tilePos);

        // 4) 기본 채굴 완료 처리 (타일 제거, 하이라이트 해제 등)
        base.FinishMining(tilePos);
    }

    public void SetToolPower(float power)
    {
        _toolPower = power;
    }
}
