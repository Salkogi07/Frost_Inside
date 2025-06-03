using UnityEngine;

namespace Script.Plyayer_22
{
    public abstract class EntityState
    {
        protected Player player;
        protected StateMachine stateMachine;
        protected string animBoolName;

        protected Animator anim;
        protected Rigidbody2D rigidbody;

        public EntityState(Player player, StateMachine stateMachine, string animBoolName)
        {
            this.player = player;
            this.stateMachine = stateMachine;
            this.animBoolName = animBoolName;

            anim = player.Anim;
            rigidbody = player.Rigidbody2D;
        }

        public virtual void Enter()
        {
            anim.SetBool(animBoolName, true);
        }

        public virtual void Update()
        {
            anim.SetFloat("yVelocity", rigidbody.linearVelocity.y);
        }

        public virtual void FiexedUpdate()
        {
            anim.SetFloat("yVelocity", rigidbody.linearVelocity.y);
        }

        public virtual void Exit()
        {
            anim.SetBool(animBoolName, false);
        }
    }
}
