using System;
using Stats;
using UnityEngine;

public class EnemyBomb_Monkey_Combat : MonoBehaviour
{
    
    public int damage = 10;
    // public Collider2D[] targetColliders;
    private Enemy_Bomb_Monkey EnemyBombMonkey;

    [Header("Target detection")] 
    [SerializeField] private Transform targetChack;
    [SerializeField] private float targetcheckRadius;
    [SerializeField] private LayerMask whatIsTarget;
    // public TagHandle;
    private void Start()
    {
        EnemyBombMonkey = GetComponent<Enemy_Bomb_Monkey>();
    }

    public void PerformAttack()
    {
        GetDetectedColliders();
        // entityHealth?.TakeDamage(damage,transform);
        foreach (var target in GetDetectedColliders())
        {
            // Player_Condition playerCondition = target.GetComponent<Player_Condition>();
            //
            // playerCondition?.TakeDamage(damage);
            Entity_Health entityHealth = target.GetComponent<Entity_Health>();
            entityHealth?.TakeDamage(damage,transform);
            // Enemy_Health enemy_Health = target.GetComponent<Enemy_Health>();
            //  
            // enemy_Health?.TakeDamage(damage,transform);
        }
    }
    
    private Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(targetChack.position, targetcheckRadius, whatIsTarget);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetChack.position, targetcheckRadius);
    }
}