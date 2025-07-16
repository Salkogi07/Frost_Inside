using Script.Plyayer_22;
using Scripts.Enemy;
using Stats;
using UnityEngine;

public class Enemy_Health : Entity_Stats
{
    private Entity_VFX entityVFX;
    private Enemy enemy => GetComponent<Enemy>();

    [Header("On Damage Knockback")]
    [SerializeField]  private float knockbackDuration = .2f;
    [SerializeField]  private Vector2 onDamageKnockback = new Vector2(1.5f, 2.5f);
    
    protected virtual void Awake()
    {
        entityVFX = GetComponent<Entity_VFX>();
    }
    
    
    
    public override void TakeDamage(float damage, Transform damageDeaaler)
    {
        if (damageDeaaler.GetComponent<Player>() != null)
        {
            enemy.TryEnterBattleState(damageDeaaler);
        }
        entityVFX?.PlayOnDamageVfx();
        enemy?.Reciveknockback(onDamageKnockback, knockbackDuration);
        base.TakeDamage(damage, damageDeaaler);
    }
    
    
    


    


}
