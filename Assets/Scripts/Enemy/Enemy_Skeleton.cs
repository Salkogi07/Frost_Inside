
    public class Enemy_Skeleton : Enemy
    {
        
        public Enemy_IdleState IdleState { get; private set; }
        public Enemy_MoveState MoveState { get; private set; }
        public Enemy_AttackState AttackState { get; private set; }
        public Enemy_ChaseState  ChaseState { get; private set; }
        public Enemy_DeadState  DeadState { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            IdleState = new Enemy_IdleState(this, EnemyStateMachine, "idle");
            MoveState = new Enemy_MoveState(this, EnemyStateMachine, "move");
            AttackState = new Enemy_AttackState(this, EnemyStateMachine, "attack");
            ChaseState = new Enemy_ChaseState(this, EnemyStateMachine, "Chase");
            DeadState = new Enemy_DeadState(this, EnemyStateMachine, "dead");
            
        }

        protected override void Start()
        {
            base.Start();
            
            EnemyStateMachine.Initialize(IdleState);
        }
    }
