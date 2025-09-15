
using UnityEngine;

public class Enemy_laboratory_robot_AnimationTrigger : MonoBehaviour
{
    private Enemy_laboratory_robot enemy;
    private Enemy_Combat enemyCombat;
    
    private void Awake()
    {
        enemy = GetComponentInParent<Enemy_laboratory_robot>();
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
