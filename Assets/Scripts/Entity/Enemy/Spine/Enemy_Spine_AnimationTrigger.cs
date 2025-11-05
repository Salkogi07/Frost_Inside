
using UnityEngine;

public class Enemy_Spine_AnimationTrigger : MonoBehaviour
{
    private Enemy_Spine enemy;
    private Enemy_Combat enemyCombat;
    
    private void Awake()
    {
        enemy = GetComponentInParent<Enemy_Spine>();
        enemyCombat = GetComponentInParent<Enemy_Combat>();
    }

    public void CurrentStateTrigger()
    {
        enemy.CallAnimationTrigger();
    }

    public void AttackTrigger()
    {
        enemyCombat.PerformAttack();
    }
    
}
