using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator Anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    
    private bool _isFacingRight = false;
    public int FacingDirection { get; private set; } = -1;
    
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
    //     this.player =  player;
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
    private bool isknocked;

    protected virtual void Awake()
    {
        Anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void HandleFlip(float xVelcoity)
    {
        if (xVelcoity > 0 && !_isFacingRight)
            Flip();
        else if (xVelcoity < 0 && _isFacingRight)
            Flip();
    }
    
    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        _isFacingRight = !_isFacingRight;
        FacingDirection *= -1;
    }
    
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isknocked)
        {
            return;
        }

        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
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