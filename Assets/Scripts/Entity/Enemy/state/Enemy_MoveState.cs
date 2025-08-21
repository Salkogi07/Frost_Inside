
    public class Enemy_MoveState : Move_director
    {
        
        public Enemy_MoveState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            
            if (enemy.IsGroundDetected == false || enemy.IsWallDetected)
                enemy.Flip();
        }

        public override void Update()
        {
            base.Update();

            if (enemy.IsGroundDetected == false|| enemy.IsWallDetected)
                enemyStateMachine.ChangeState(enemy.IdleDirector);
            
            
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();
            
            enemy.SetVelocity(enemy.MoveSpeed * enemy.FacingDirection, rigidbody.linearVelocity.y);
        }
        
    }
