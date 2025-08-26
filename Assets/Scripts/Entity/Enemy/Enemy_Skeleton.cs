
    using UnityEngine;

    public class Enemy_Skeleton : Enemy
    {

        
        public Enemy_IdleState IdleState { get; private set; }
        public Enemy_MoveState MoveState { get; private set; } 
        public Enemy_BattleState  BattleState { get; private set; }
        public Enemy_JumpState JumpState { get; private set; }
        public Enemy_AttackState AttackState {get; private set;}
        // public Enemy_GroundedState GroundedState;
        public Enemy_DeadState DeadState { get; private set; }
        
        [Header("Jump Config")] 
        public EnemyJumpData jumpData;
        
        // [System.Serializable]
        // public class EnemyJumpData
        // {
        //     public float jumpForce = 7f;
        //     public float jumpCoolTime = 2f;
        //     [HideInInspector]public bool isJumping = false;
        // }
        protected override void Awake()
        {
            base.Awake();
            jumpData = new EnemyJumpData();
            jumpData = jumpData.Clone();
            States[typeof(Enemy_AttackState)] = new Enemy_AttackState(this, EnemyStateMachine, "attack");
            IdleState = new Enemy_IdleState(this, EnemyStateMachine, "idle");
            MoveState = new Enemy_MoveState(this, EnemyStateMachine, "move");
            BattleState = new Enemy_BattleState(this, EnemyStateMachine, "battle");
            // GroundedState = new Enemy_GroundedState(this, EnemyStateMachine,null);
            // JumpState = new Enemy_JumpState(this,EnemyStateMachine, "jump", jumpData);
            States[typeof(Enemy_JumpState)] = new Enemy_JumpState(this,EnemyStateMachine, "jump", jumpData);
            
            DeadState = new Enemy_DeadState(this, EnemyStateMachine, "dead");
            
            
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
