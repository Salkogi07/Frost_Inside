using UnityEngine;



public abstract class Enemy_laboratory_robot_State
{
    protected Enemy_laboratory_robot laboratoryRobot;
    protected Enemy_laboratory_robot_StateMachine enemyStateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected bool triggerCalled;

    public Enemy_laboratory_robot_State(Enemy_laboratory_robot laboratoryRobot, Enemy_laboratory_robot_StateMachine enemyStateMachine, string animBoolName)
    {
        this.laboratoryRobot = laboratoryRobot;
        this.enemyStateMachine = enemyStateMachine;
        this.animBoolName = animBoolName;

        anim = laboratoryRobot.Anim;
        rb = laboratoryRobot.rb;
    }

    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;

        float battleAnimSpeedMultiplier = laboratoryRobot.battleMoveSpeed / laboratoryRobot.MoveSpeed;

        anim.SetFloat("BattleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        anim.SetFloat("moveAnimSpeedMultiplier", laboratoryRobot.moveAnimSpeedMultiplier);
        // anim.SetFloat("xVelocity", rigidbody.linearVelocity.x);
        anim.SetFloat("xVelocity", Mathf.Clamp(rb.linearVelocityX, -1f, 1f));
    }

    public virtual void FiexedUpdate()
    {
        anim.SetFloat("moveAnimSpeedMultiplier", laboratoryRobot.moveAnimSpeedMultiplier);
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