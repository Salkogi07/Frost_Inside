using System;
using Script.Plyayer_22;
using Stats;
using UnityEngine;
public class attack_test : MonoBehaviour

{
    private Enemy_Combat _enemyCombat;

    private void Awake()
    {
        _enemyCombat = GetComponent<Enemy_Combat>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _enemyCombat.PerformAttack();
        }
    }
    
    // public int damage = 10;
    // // public Collider2D[] targetColliders;
    //
    //
    //
    // [Header("Target detection")] 
    // [SerializeField] private Transform targetChack;
    // [SerializeField] private float targetcheckRadius;
    // [SerializeField] private LayerMask whatIsTarget;
    // // public TagHandle;
    // public void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Q))
    //     {
    //         PerformAttack();
    //         
    //     }
    //     
    // }
    //
    //
    // public void PerformAttack()
    // {
    //     GetDetectedColliders();
    //     
    //     foreach (var target in GetDetectedColliders())
    //     {
    //         Debug.Log("음 이 티에이티스야");
    //         Entity_Health entityHealth = target.GetComponent<Entity_Health>();
    //         Debug.Log(transform);
    //         entityHealth?.TakeDamage(damage,transform);
    //         // Enemy_Health enemy_Health = target.GetComponent<Enemy_Health>();
    //         //  
    //         // enemy_Health?.TakeDamage(damage,transform);
    //     }
    // }
    //
    // private Collider2D[] GetDetectedColliders()
    // {
    //     return Physics2D.OverlapCircleAll(targetChack.position, targetcheckRadius, whatIsTarget);
    // }
    //
    // private void OnDrawGizmos()
    // {
    //     Gizmos.DrawWireSphere(targetChack.position, targetcheckRadius);
    // }
}
