
    public class Enemy_Skeleton : Enemy
    {

        
        public Enemy_IdleState IdleState { get; private set; }
        public Enemy_MoveState MoveState { get; private set; } 
        public Enemy_BattleState  BattleState { get; private set; }
        public Enemy_AttackState AttackState {get; private set;}
        // public Enemy_GroundedState GroundedState;
        public Enemy_DeadState DeadState { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            // States[typeof(Enemy_IdleState)] = new Enemy_IdleState(this, EnemyStateMachine, "idle");
            // States[typeof(Enemy_MoveState)] = new Enemy_MoveState(this, EnemyStateMachine, "move");
            States[typeof(Enemy_AttackState)] = new Enemy_AttackState(this, EnemyStateMachine, "attack");
            // States[typeof(Enemy_ChaseState)] = new Enemy_BattleState(this, EnemyStateMachine, "Battle");
            // States[typeof(Enemy_DeadState)] = new Enemy_DeadState(this, EnemyStateMachine, "dead");
            IdleState = new Enemy_IdleState(this, EnemyStateMachine, "idle");
            MoveState = new Enemy_MoveState(this, EnemyStateMachine, "move");
            BattleState = new Enemy_BattleState(this, EnemyStateMachine, "battle");
            // GroundedState = new Enemy_GroundedState(this, EnemyStateMachine,null);
            // AttackState = new Enemy_AttackState(this, EnemyStateMachine, "attack");
            DeadState = new Enemy_DeadState(this, EnemyStateMachine, "dead");
            
            // States[typeof(Enemy_IdleState)] = IdleState;
            // States[typeof(Enemy_MoveState)] = MoveState;
            States[typeof(Enemy_BattleState)] = BattleState;
            // States[typeof(Enemy_AttackState)] = AttackState;
            // States[typeof(Enemy_DeadState)] = DeadState;
            
            IdleDirector = IdleState;
            MoveDirector = MoveState;
            ChaseDirector = BattleState;
            LifeDirector = DeadState;
            // GroundedDirector = GroundedState;

        }

        protected virtual void Start()
        {
            base.Start();
            // EnemyStateMachine.Initialize(GetState<Enemy_IdleState>());
            EnemyStateMachine.Initialize(IdleDirector);
        }
    }
