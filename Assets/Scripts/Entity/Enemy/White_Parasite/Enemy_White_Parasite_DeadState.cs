using UnityEngine;


    public class Enemy_White_Parasite_DeadState : Enemy_White_Parasite_State
    {
        private Collider2D col;


        public Enemy_White_Parasite_DeadState(Enemy_White_Parasite whiteParasite, Enemy_White_Parasite_StateMachine enemyStateMachine, string animBoolName) : base(whiteParasite, enemyStateMachine, animBoolName)
        {
            col = whiteParasite.GetComponent<Collider2D>();
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

