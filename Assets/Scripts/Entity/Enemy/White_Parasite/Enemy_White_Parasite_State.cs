using UnityEngine;



public abstract class Enemy_White_Parasite_State
{
    protected Enemy_White_Parasite whiteParasite;
    protected Enemy_White_Parasite_StateMachine enemyStateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected bool triggerCalled;

    public Enemy_White_Parasite_State(Enemy_White_Parasite whiteParasite, Enemy_White_Parasite_StateMachine enemyStateMachine, string animBoolName)
    {
        this.whiteParasite = whiteParasite;
        this.enemyStateMachine = enemyStateMachine;
        this.animBoolName = animBoolName;

        anim = whiteParasite.Anim;
        rb = whiteParasite.rb;
    }

    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;

        float battleAnimSpeedMultiplier = whiteParasite.battleMoveSpeed / whiteParasite.MoveSpeed;

        anim.SetFloat("BattleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        anim.SetFloat("moveAnimSpeedMultiplier", whiteParasite.moveAnimSpeedMultiplier);
        // anim.SetFloat("xVelocity", rigidbody.linearVelocity.x);
        anim.SetFloat("xVelocity", Mathf.Clamp(rb.linearVelocityX, -1f, 1f));
    }

    public virtual void FiexedUpdate()
    {
        anim.SetFloat("moveAnimSpeedMultiplier", whiteParasite.moveAnimSpeedMultiplier);
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