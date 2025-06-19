using Script.Plyayer_22;
using UnityEngine;

namespace Scripts.Enemy
{
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
            rigidbody = enemy.Rigidbody;
        }

        public virtual void Enter()
        {
            anim.SetBool(animBoolName, true);
            triggerCalled = false;
        }

        public virtual void Update()
        {
            stateTimer -= Time.deltaTime;
            anim.SetFloat("moveAnimSpeedMultiplier",enemy.moveAnimSpeedMultiplier);
            // anim.SetFloat("yVelocity", rigidbody.linearVelocity.y);
        }

        public virtual void FiexedUpdate()
        {
            anim.SetFloat("moveAnimSpeedMultiplier",enemy.moveAnimSpeedMultiplier);
            // anim.SetFloat("yVelocity", rigidbody.linearVelocity.y);
        }

        public virtual void Exit()
        {
            anim.SetBool(animBoolName, false);
        }
        
        // public void Ca
    }
}