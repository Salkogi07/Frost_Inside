using Script.Plyayer_22;
using Scripts.Enemy;
using UnityEngine;

public class Enemy_Health : Entity_Health
{
    
    private Enemy enemy => GetComponent<Enemy>();

    private void Awake()
    {
        // TakeDamage(10,transform);
    }
    
    
    
    public override void TakeDamage(int damage, Transform damageDeaaler)
    {
        if (damageDeaaler.GetComponent<Player>() != null)
        {
            
            enemy.TryEnterBattleState(damageDeaaler);
        }
        
        // Debug.Log(damageDeaaler);
        base.TakeDamage(damage, damageDeaaler);
    }
}
