
using UnityEngine;

public class EnemyAnimationTrigger : MonoBehaviour
{
    private Entity entity;
    private Enemy_Combat enemyCombat;
    
    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
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
