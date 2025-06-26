using Scripts.Enemy;
using UnityEngine;

public class EnemyAnimationTrigger : MonoBehaviour
{
    private Enemy entity;
    private Enemy_Combat enemyCombat;
    
    private void Awake()
    {
        entity = GetComponentInParent<Enemy>();
        enemyCombat = GetComponentInParent<Enemy_Combat>();
    }

    public void CurrentStateTrigger()
    {
        entity.CallAnimationTrigger();
    }

    public void AttackTrigger()
    {
        enemyCombat.PerformAttack();
    }
    
}
