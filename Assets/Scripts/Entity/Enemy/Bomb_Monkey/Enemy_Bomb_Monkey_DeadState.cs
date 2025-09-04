using UnityEngine;


    public class Enemy_Bomb_Monkey_DeadState : Enemy_Bomb_Monkey_State
{
        private Collider2D col;

    public Enemy_Bomb_Monkey_DeadState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine, string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
        col = bombMonkey.GetComponent<Collider2D>();
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
            Object.Destroy(bombMonkey.gameObject);
        }
    }

