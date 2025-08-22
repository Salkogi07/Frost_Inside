using UnityEngine;

public abstract class EnemyState
{
    protected Enemy enemy;
    protected Enemy_StateMachine enemyStateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody2D rigidbody;

    protected float stateTimer;
    protected bool triggerCalled;

    public EnemyState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
        this.animBoolName = animBoolName;

        anim = enemy.Anim;
        rigidbody = enemy.rb;
    }

    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;

        float battleAnimSpeedMultiplier = enemy.battleMoveSpeed / enemy.MoveSpeed;

        anim.SetFloat("BattleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        anim.SetFloat("moveAnimSpeedMultiplier", enemy.moveAnimSpeedMultiplier);
        // anim.SetFloat("xVelocity", rigidbody.linearVelocity.x);
        anim.SetFloat("xVelocity", Mathf.Clamp(rigidbody.linearVelocityX, -1f, 1f));
    }

    public virtual void FiexedUpdate()
    {
        anim.SetFloat("moveAnimSpeedMultiplier", enemy.moveAnimSpeedMultiplier);
        // anim.SetFloat("yVelocity", rigidbody.linearVelocity.y);
    }

    public virtual void Exit()
    {
        anim.SetBool(animBoolName, false);
    }

    public void CallAnimationTrigger()
    {
        triggerCalled = true;
    }
}