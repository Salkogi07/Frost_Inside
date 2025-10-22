using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class OtherTilingRule : RuleTile.TilingRuleOutput
{
    public enum Neighbor { DontCare, Is, IsNot };
    
    public TileBase m_OtherTile;
    public List<Neighbor> m_Neighbors = new List<Neighbor>(new Neighbor[9]);
}


[CreateAssetMenu(fileName = "New Custom Rule Tile", menuName = "Tiles/Custom Rule Tile")]
public class CustomRuleTile : RuleTile
{
    [HideInInspector]
    public List<OtherTilingRule> m_OtherTilingRules = new List<OtherTilingRule>();

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        // GetTileData 호출 시, 타일에 적용할 규칙을 먼저 찾습니다.
        var iden = Matrix4x4.identity;
        foreach (var rule in m_TilingRules)
        {
            if (RuleMatches(rule, position, tilemap, ref iden))
            {
                ApplyRule(rule, ref tileData, iden);
                return;
            }
        }

        foreach (var rule in m_OtherTilingRules)
        {
            if (RuleMatches(rule, position, tilemap))
            {
                ApplyRule(rule, position, ref tileData);
                return;
            }
        }
        
        base.GetTileData(position, tilemap, ref tileData);
    }
    
    // 기본 TilingRule을 적용하기 위한 private 메소드
    private void ApplyRule(TilingRule rule, ref TileData tileData, Matrix4x4 transform)
    {
        tileData.sprite = rule.m_Sprites[0]; // 기본 규칙은 첫 번째 스프라이트만 사용
        tileData.gameObject = rule.m_GameObject;
        tileData.colliderType = rule.m_ColliderType;
        tileData.transform = transform;
    }
    
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
                
                bool isOtherTile = false;
                // m_OtherTile이 CustomRuleTile(혹은 RuleTile) 타입인지 확인
                if (rule.m_OtherTile is RuleTile otherRuleTile)
                {
                    // RuleTile의 RuleMatch를 사용하여 안정적으로 비교
                    isOtherTile = otherRuleTile.RuleMatch(0, neighborTile);
                }
                else
                {
                    // 일반 TileBase인 경우, 기존처럼 참조로 비교
                    isOtherTile = (neighborTile == rule.m_OtherTile);
                }

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