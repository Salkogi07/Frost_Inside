using Script.Plyayer_22;
using Scripts.Enemy;
using Stats;
using UnityEngine;

public class Entity_Health : MonoBehaviour
{
    private Entity_VFX _entityVFX;
    private Enemy enemy;
    

    [SerializeField] protected float health;
    [SerializeField] protected bool isDead;
    [Header("On Damage Knockback")]
    [SerializeField]  private float knockbackDuration = .2f;
    [SerializeField]  private Vector2 onDamageKnockback = new Vector2(1.5f, 2.5f);
    
    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        _entityVFX = GetComponent<Entity_VFX>();
        // TakeDamage(10,transform);
    }
    
    
    
    
    
    public virtual void TakeDamage(int damage , Transform damageDealer)
    {
        if (_entityVFX != null)
        {
            Debug.Log("딱딱게이",damageDealer);
            
        }
        else
        {
            Debug.Log("애미",damageDealer);
            
        }
        if (isDead)
        {
            return;
        }
        // Vector2 knockback = CalculateKnockback(damageDealer);
        //
        // enemy?.Reciveknockback(knockback, knockbackDuration);
        
        
        _entityVFX?.PlayOnDamageVfx();
        ReduceHp(damage);
        // base.TakeDamage(damage, damageDeaaler);
    }

    private void ReduceHp(float damage)
    {
        health -= damage;
        // Debug.Log(health);
        if (health < 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("뻐@거");
    }


    private Vector2 CalculateKnockback(Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;
        
        Vector2 knockback = onDamageKnockback;
        
        knockback.x = knockback.x * direction;
        
        return knockback;
    }
    
    


}
