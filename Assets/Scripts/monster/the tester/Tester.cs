


    public class Tester : Enemy
    {
        public Enemy_IdleState IdleState { get; private set; }
        public Enemy_MoveState MoveState { get; private set; }
        public Enemy_ChaseState  ChaseState { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            IdleState = new Enemy_IdleState(this, EnemyStateMachine, "idle");
            MoveState = new Enemy_MoveState(this, EnemyStateMachine, "move");
            ChaseState = new Enemy_ChaseState(this, EnemyStateMachine, "Chase");
        }

        protected override void Start()
        {
            base.Start();

            EnemyStateMachine.Initialize(IdleState);
        }
    }

