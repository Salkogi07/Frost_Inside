// using UnityEngine;
//
// public class Enemy_Health : Entity_Health
// {
//     
//     private Enemy enemy => GetComponent<Enemy>();
//
//     private void Awake()
//     {
//         // TakeDamage(10,transform);
//     }
//     
//     
//     
//     public override void TakeDamage(int damage, Transform damageDeaaler)
//     {
//         
//         base.TakeDamage(damage, damageDeaaler);
//         
//         
//         if (isDead)
//         {
//             return;
//         }
//         if (damageDeaaler.GetComponent<Player>() != null)
//         {
//             
//             enemy.TryEnterBattleState(damageDeaaler);
//         }
//         
//         // Debug.Log(damageDeaaler);
//         
//     }
// }
