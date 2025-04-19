using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Player_TileMining : BaseTileMiner
{
    [Header("Tool Settings")]
    public float toolPower = 10f;          // 플레이어의 채굴력

    protected override void Awake()
    {
        base.Awake();
        // 추가 초기화 예: 사운드 매니저, 이펙트 준비 등
    }

    protected override void UpdateMining(Vector3Int tilePos)
    {
        if (!tileAlphaDict.ContainsKey(tilePos))
            tileAlphaDict[tilePos] = 1f;

        var map = GetTilemapAt(tilePos);
        var tileBase = map.GetTile(tilePos);

        // 방어력 조회
        float defense = miningSettings.GetDefense(tileBase);
        // 채굴 시간 = miningTime × (defense ÷ toolPower)
        float timeToMine = miningTime * (defense / Mathf.Max(toolPower, 0.0001f));
        // alpha 감소량 = 1 ÷ timeToMine 초 만큼 줄어들도록
        float decrease = Time.deltaTime / timeToMine;

        tileAlphaDict[tilePos] -= decrease;

        // 색상 적용 (투명도 조절)  
        ApplyTileAlpha(tilePos, Mathf.Clamp01(tileAlphaDict[tilePos]));

        if (tileAlphaDict[tilePos] <= 0f)
            FinishMining(tilePos);

        // TODO: 채굴 이펙트·사운드 재생
    }

    // 필요 시 HandleMining, ApplyTileAlpha 등을 오버라이드하여 확장 가능
}
