using Script.Plyayer_22;
using Scripts.Enemy;
using Stats;
using UnityEngine;

public class Enemy_Health : Entity_Stats
{
    private Enemy enemy => GetComponent<Enemy>();
    
    
    
    public override void TakeDamage(float damage, Transform damageDeaaler)
    {
        if (damageDeaaler.GetComponent<Player>() != null)
        {
            enemy.TryEnterBattleState(damageDeaaler);
        }
        base.TakeDamage(damage, damageDeaaler);
    }
    
    
    


    


}
