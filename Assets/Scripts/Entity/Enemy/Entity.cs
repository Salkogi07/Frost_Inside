using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Entity : NetworkBehaviour
{
    public Animator Anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    
    // public float lastTimeWasInBattle;
    // public float inGameTime;


    // Enemy 코드를 따로 만들어야함 EntityDeath() 
    // ovrrive

    // public void TryEnterBattleState(Transform player)
    // {
    //     if (EnemyStateMachine.currentState == BattleState)
    //     {
    //         return;
    //     }
    //     if(EnemyStateMachine.currentState == AttackState)
    //     {
    //         return;
    //     }
    //     this.player = player;
    //     EnemyStateMachine.ChangeState(BattleState);
    // }

    // public Transform GetPlayerReference()
    // {
    //     if (player == null)
    //     {
    //         player = PlayerDetection().transform;            
    //     }
    //     return player;
    // }
    
    private Coroutine KnockbackCo;
    protected bool isknocked;

    protected virtual void Awake()
    {
        Anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void Reciveknockback(Vector2 knockback, float duration)
    {
        if (KnockbackCo != null)
        {
            StopCoroutine(KnockbackCo);
        }

        KnockbackCo = StartCoroutine(konckbackCo(knockback, duration));
    }


    private IEnumerator konckbackCo(Vector2 knockback, float duration)
    {
        isknocked = true;
        rb.linearVelocity = knockback;
        yield return new WaitForSeconds(duration);
        rb.linearVelocity = Vector2.zero;
        isknocked = false;
    }
}