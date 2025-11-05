using UnityEngine;


    public class Enemy_Spine_DeadState : Enemy_Spine_State
    {
        private Collider2D col;


        public Enemy_Spine_DeadState(Enemy_Spine enemySpine, Enemy_Spine_StateMachine enemyStateMachine, string animBoolName) : base(enemySpine, enemyStateMachine, animBoolName)
        {
            col = enemySpine.GetComponent<Collider2D>();
        }

        public override void Enter()
        {
            base.Enter();
            //애니메이션이 멈춤
            // anim.enabled = false;
            // col.enabled = false;
            //
            // rb.gravityScale = 12;
            // rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15);
            Debug.Log("Entered");
            enemyStateMachine.SwitchOffStateMachine();
        }
    }

