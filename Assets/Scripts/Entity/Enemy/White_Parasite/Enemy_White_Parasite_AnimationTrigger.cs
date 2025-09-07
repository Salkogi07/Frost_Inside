
using UnityEngine;

public class Enemy_White_Parasite_AnimationTrigger : MonoBehaviour
{
    private Enemy_White_Parasite enemy;
    private Enemy_Combat enemyCombat;
    
    private void Awake()
    {
        enemy = GetComponentInParent<Enemy_White_Parasite>();
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
