using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 새로운 규칙 클래스입니다. RuleTile.TilingRuleOutput을 상속받아
// GameObject, Collider, Output(Single, Random, Animation) 등의 속성을 그대로 사용합니다.
[System.Serializable]
public class OtherTilingRule : RuleTile.TilingRuleOutput
{
    public enum Neighbor { DontCare, Is, IsNot };

    // 검사 대상이 될 다른 타일 에셋
    public TileBase m_OtherTile;
    // 3x3 격자에 대한 9개의 이웃 규칙
    public List<Neighbor> m_Neighbors = new List<Neighbor>(new Neighbor[9]);
}


[CreateAssetMenu(fileName = "New Custom Rule Tile", menuName = "Tiles/Custom Rule Tile")]
public class CustomRuleTile : RuleTile
{
    // 커스텀 에디터에서 사용할 새로운 규칙 리스트
    [HideInInspector]
    public List<OtherTilingRule> m_OtherTilingRules = new List<OtherTilingRule>();

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        // 'Other Tiling Rules'를 먼저 검사합니다.
        foreach (var rule in m_OtherTilingRules)
        {
            if (RuleMatches(rule, position, tilemap))
            {
                ApplyRule(rule, position, ref tileData);
                return;
            }
        }

        // 일치하는 'Other' 규칙이 없으면, 기본 'Tiling Rules' 로직을 실행합니다.
        base.GetTileData(position, tilemap, ref tileData);
    }

    // 규칙에 따라 타일 데이터를 설정합니다. Random, Animation 출력을 지원하도록 수정됩니다.
    private void ApplyRule(OtherTilingRule rule, Vector3Int position, ref TileData tileData)
    {
        tileData.gameObject = rule.m_GameObject;
        tileData.colliderType = rule.m_ColliderType;
        tileData.transform = Matrix4x4.identity;

        switch (rule.m_Output)
        {
            case TilingRuleOutput.OutputSprite.Single:
                tileData.sprite = rule.m_Sprites[0];
                break;
            case TilingRuleOutput.OutputSprite.Random:
                int index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) * rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
                tileData.sprite = rule.m_Sprites[index];
                break;
            case TilingRuleOutput.OutputSprite.Animation:
                tileData.sprite = rule.m_Sprites[0];
                break;
        }
    }

    // 애니메이션 데이터를 가져오는 부분을 오버라이드합니다.
    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
    {
        foreach (var rule in m_OtherTilingRules)
        {
            if (rule.m_Output == TilingRuleOutput.OutputSprite.Animation && RuleMatches(rule, position, tilemap))
            {
                tileAnimationData.animatedSprites = rule.m_Sprites;
                tileAnimationData.animationSpeed = Random.Range(rule.m_MinAnimationSpeed, rule.m_MaxAnimationSpeed);
                return true;
            }
        }
        // 기본 애니메이션 로직도 실행되도록 base 호출을 추가합니다.
        return base.GetTileAnimationData(position, tilemap, ref tileAnimationData);
    }

    private bool RuleMatches(OtherTilingRule rule, Vector3Int position, ITilemap tilemap)
    {
        if (rule.m_OtherTile == null || rule.m_Neighbors.Count < 9) return false;

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) continue;

                int index = (1 - y) * 3 + (x + 1);
                OtherTilingRule.Neighbor neighborRule = rule.m_Neighbors[index];

                if (neighborRule == OtherTilingRule.Neighbor.DontCare) continue;

                Vector3Int neighborPos = new Vector3Int(position.x + x, position.y + y, position.z);
                TileBase neighborTile = tilemap.GetTile(neighborPos);
                bool isOtherTile = (neighborTile == rule.m_OtherTile);

                if ((neighborRule == OtherTilingRule.Neighbor.Is && !isOtherTile) ||
                    (neighborRule == OtherTilingRule.Neighbor.IsNot && isOtherTile))
                {
                    return false;
                }
            }
        }
        return true;
    }
}