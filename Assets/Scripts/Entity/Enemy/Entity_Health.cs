using UnityEngine;

public class Entity_Health : MonoBehaviour
{
    private Entity_VFX _entityVFX;
    private Entity entity;
    

    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth = 100; 
    [SerializeField] protected bool isDead;
    [Header("On Damage Knockback")]
    [SerializeField]  private float knockbackDuration = .2f;
    [SerializeField]  private Vector2 KnockbackPower = new Vector2(1.5f, 2.5f);
    [SerializeField]  private Vector2 heavyKnockbackpower = new Vector2(7f, 7f);
    [SerializeField]  private float heavyKnockbackDuration = .5f;
    [Header("On Heavy Damage")]
    [SerializeField]  private float heavyDamageThreshold = .3f;
    
    protected virtual void Awake()
    {
        entity = GetComponent<Entity>();
        _entityVFX = GetComponent<Entity_VFX>();
        
        
        health = maxHealth;
    }
    
    
    
    
    
    public virtual void TakeDamage(int damage , Transform damageDealer)
    {
        // if (_entityVFX != null)
        // {
        //     Debug.Log("딱딱게이",damageDealer);
        //     
        // }
        // else
        // {
        //     Debug.Log("애미",damageDealer);
        //     
        // }
        
        Vector2 knockback = CalculateKnockback(damage,damageDealer);
        float duration = CalculateDuration(damage);
        
        
        // enemy?.Reciveknockback(knockback, knockbackDuration);
        
        // 대미지 비례 넉백량 증가
        entity?.Reciveknockback(knockback, duration);
        
        _entityVFX?.PlayOnDamageVfx();
        ReduceHp(damage);
        
    }

    private void ReduceHp(float damage)
    {
        health -= damage;
        // Debug.Log(health);
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
    }

    
    // 대미지 비례 넉백량 증가
    private float CalculateDuration(float damage) => IsHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration;
    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;
        
        Vector2 knockback = IsHeavyDamage(damage) ? heavyKnockbackpower : KnockbackPower;
        
        knockback.x = knockback.x * direction;
        
        return knockback;
    }
    // 대미지 비례 넉백량 증가
    private bool IsHeavyDamage(float damage) => damage / maxHealth > heavyDamageThreshold;
}
