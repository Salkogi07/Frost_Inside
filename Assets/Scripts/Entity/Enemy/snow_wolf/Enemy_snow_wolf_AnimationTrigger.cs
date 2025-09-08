
using UnityEngine;

public class Enemy_snow_wolf_AnimationTrigger : MonoBehaviour
{
    private Enemy_snow_wolf enemy;
    private Enemy_Combat enemyCombat;
    
    private void Awake()
    {
        enemy = GetComponentInParent<Enemy_snow_wolf>();
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
