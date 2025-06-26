using System;
using Stats;
using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    
    public int damage = 10;
    // public Collider2D[] targetColliders;

    [Header("Tatget detection")] 
    [SerializeField] private Transform targetChack;
    [SerializeField] private float targetcheckRadius;
    [SerializeField] private LayerMask whatIsTarget;

    public void PerformAttack()
    {
        GetDetectedColliders();

        foreach (var target in GetDetectedColliders())
        {
            Player_Condition playerCondition = target.GetComponent<Player_Condition>();
             
            playerCondition?.TakeDamage(damage);
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