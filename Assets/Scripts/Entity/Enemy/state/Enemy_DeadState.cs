using UnityEngine;


    public class Enemy_DeadState : Life_director
    {
        private Collider2D col;
        
        public Enemy_DeadState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
        {
            col = enemy.GetComponent<Collider2D>();
        }

        public override void Enter()
        {
            //애니메이션이 멈춤
            anim.enabled = false;
            col.enabled = false;
            
            rigidbody.gravityScale = 12;
            rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 15);
            
            enemyStateMachine.SwitchOffStateMachine();
        }
    }

