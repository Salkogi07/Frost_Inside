using UnityEngine;


    public class Enemy_snow_wolf_DeadState : Enemy_snow_wolf_State
    {
        private Collider2D col;


        public Enemy_snow_wolf_DeadState(Enemy_snow_wolf SnowWolf, Enemy_snow_wolf_StateMachine enemyStateMachine, string animBoolName, Collider2D col) : base(SnowWolf, enemyStateMachine, animBoolName)
        {
            this.col = col;
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

