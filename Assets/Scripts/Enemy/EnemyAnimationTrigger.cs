using Scripts.Enemy;
using UnityEngine;

public class EnemyAnimationTrigger : MonoBehaviour
{
    private Enemy entity;

    private void Awake()
    {
        entity = GetComponentInParent<Enemy>();
    }

    public void CurrentStateTrigger()
    {
        entity.CallAnimationTrigger();
    }
    
}
