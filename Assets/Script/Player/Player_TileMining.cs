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

        // 방어력(defense)은 ScriptableObject나 다른 설정에서 가져온다고 가정
        float defense = 0f; // 예: miningSettings.GetDefense(tilemap.GetTile(tilePos));
        float effectivePower = Mathf.Max(toolPower - defense, 0f);

        float decrease = (effectivePower / miningTime) * Time.deltaTime;
        tileAlphaDict[tilePos] -= decrease;

        float alpha = tileAlphaDict[tilePos];
        ApplyTileAlpha(tilePos, alpha);

        if (alpha <= 0f)
            FinishMining(tilePos);

        // 추가: 이펙트나 사운드 재생
    }

    // 필요 시 HandleMining, ApplyTileAlpha 등을 오버라이드하여 확장 가능
}
