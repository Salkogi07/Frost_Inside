using UnityEngine;



public abstract class Enemy_Spine_State
{
    protected Enemy_Spine enemySpine;
    protected Enemy_Spine_StateMachine enemyStateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected bool triggerCalled;

    public Enemy_Spine_State(Enemy_Spine enemySpine, Enemy_Spine_StateMachine enemyStateMachine, string animBoolName)
    {
        this.enemySpine = enemySpine;
        this.enemyStateMachine = enemyStateMachine;
        this.animBoolName = animBoolName;

        anim = enemySpine.Anim;
        rb = enemySpine.rb;
    }

    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;

        float battleAnimSpeedMultiplier = enemySpine.battleMoveSpeed / enemySpine.MoveSpeed;

        anim.SetFloat("BattleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        anim.SetFloat("moveAnimSpeedMultiplier", enemySpine.moveAnimSpeedMultiplier);
        // anim.SetFloat("xVelocity", rigidbody.linearVelocity.x);
        anim.SetFloat("xVelocity", Mathf.Clamp(rb.linearVelocityX, -1f, 1f));
    }

    public virtual void FiexedUpdate()
    {
        anim.SetFloat("moveAnimSpeedMultiplier", enemySpine.moveAnimSpeedMultiplier);
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