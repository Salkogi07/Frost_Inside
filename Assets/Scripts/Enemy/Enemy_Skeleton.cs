
    public class Enemy_Skeleton : Enemy
    {
        
        
        protected override void Awake()
        {
            base.Awake();
            IdleState = new Enemy_IdleState(this, EnemyStateMachine, "idle");
            MoveState = new Enemy_MoveState(this, EnemyStateMachine, "move");
            AttackState = new Enemy_AttackState(this, EnemyStateMachine, "attack");
            BattleState = new Enemy_BattleState(this, EnemyStateMachine, "battle");
            DeadState = new Enemy_DeadState(this, EnemyStateMachine, "dead");
        }

        protected override void Start()
        {
            base.Start();
            
            EnemyStateMachine.Initialize(IdleState);
        }
    }
