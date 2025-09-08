using UnityEngine;



public abstract class Enemy_snow_wolf_State
{
    protected Enemy_snow_wolf SnowWolf;
    protected Enemy_snow_wolf_StateMachine enemyStateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected bool triggerCalled;

    public Enemy_snow_wolf_State(Enemy_snow_wolf SnowWolf, Enemy_snow_wolf_StateMachine enemyStateMachine, string animBoolName)
    {
        this.SnowWolf = SnowWolf;
        this.enemyStateMachine = enemyStateMachine;
        this.animBoolName = animBoolName;

        anim = SnowWolf.Anim;
        rb = SnowWolf.rb;
    }

    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;

        float battleAnimSpeedMultiplier = SnowWolf.battleMoveSpeed / SnowWolf.MoveSpeed;

        anim.SetFloat("BattleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        anim.SetFloat("moveAnimSpeedMultiplier", SnowWolf.moveAnimSpeedMultiplier);
        // anim.SetFloat("xVelocity", rigidbody.linearVelocity.x);
        anim.SetFloat("xVelocity", Mathf.Clamp(rb.linearVelocityX, -1f, 1f));
    }

    public virtual void FiexedUpdate()
    {
        anim.SetFloat("moveAnimSpeedMultiplier", SnowWolf.moveAnimSpeedMultiplier);
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