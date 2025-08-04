using UnityEngine;



    public class Enemy : Entity
    {
        // public Animator Anim { get; private set; }
        // public Rigidbody2D rb { get; private set; }
        
        // public Enemy_StateMachine EnemyStateMachine;


        public Enemy_IdleState IdleState;
        public Enemy_MoveState MoveState;
        public Enemy_AttackState AttackState;
        public Enemy_BattleState BattleState;
        public Enemy_DeadState DeadState;
        
        // public Enemy_IdleState IdleState { get; private set; }
        // public Enemy_MoveState MoveState { get; private set; }
        //  public Enemy_AttackState AttackState { get; private set; }
        //  public Enemy_BattleState  BattleState { get; private set; }
        //  public Enemy_DeadState  DeadState { get; private set; }



         [Header("Battle details")] 
         public float battleMoveSpeed = 3;
         public float attackDistance = 2;
         public float battleTimeDuration = 5;
         public float minRetreatDistance = 1;
         public Vector2 retreatVelocity;
         // public float lastTimeWasInBattle;
         // public float inGameTime;
         
         [Header("Player detection")]
         [SerializeField] private Transform playerCheck;
         [SerializeField] private LayerMask whatIsPlayer;
         [SerializeField] private float playerCheckDistance = 10;
         
        [Header("Movement details")]
        public float IdleTime = 2;
        public float MoveSpeed = 1.4f;
        
        [Range(0, 10)]
        public float moveAnimSpeedMultiplier = 1;
        
        

        

        public Transform player {get; private set;}

        
        public override void EntityDeath()
        {
            base.EntityDeath();
           
            
            EnemyStateMachine.ChangeState(DeadState);
        }
        public void TryEnterBattleState(Transform player)
        {
            if (EnemyStateMachine.currentState == BattleState)
            {
                return;
            }
            if(EnemyStateMachine.currentState == AttackState)
            {
                return;
            }
            this.player =  player;
            EnemyStateMachine.ChangeState(BattleState);
        }

        public Transform GetPlayerReference()
        {
            if (player == null)
            {
                player = PlayerDetection().transform;            
            }
            return player;
        }
        public RaycastHit2D PlayerDetection()
        {
            RaycastHit2D hit = Physics2D.Raycast(playerCheck.position,Vector2.right * FacingDirection , playerCheckDistance, whatIsPlayer | whatIsGround);
        
            if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                return default;
            }
            
            return hit;
        }
        
        // public bool IsGroundDetected { get; private set; }
        // public bool IsWallDetected { get; private set; }
        // [SerializeField] private Transform primaryWallCheck;
        // [SerializeField] private Transform secondaryWallCheck;
        // [SerializeField] private LayerMask whatIsWall;
        // [SerializeField] private float wallCheckDistance;

        // private Coroutine KnockbackCo;
        // private bool isknocked;
        protected virtual void Awake()
        {
            base.Awake();
            // Anim = GetComponentInChildren<Animator>();
            // rb = GetComponent<Rigidbody2D>();
            //
            // EnemyStateMachine = new Enemy_StateMachine();
        
            // IdleState = new Enemy_IdleState(this, EnemyStateMachine, "idle");
            // MoveState = new Enemy_MoveState(this, EnemyStateMachine, "move");
            // AttackState = new Enemy_AttackState(this, EnemyStateMachine, "attack");
            // BattleState = new Enemy_BattleState(this, EnemyStateMachine, "battle");
            // DeadState = new Enemy_DeadState(this, EnemyStateMachine, "dead");
        }
        

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            base.Update();
        }
        
        

        
        
        
        // public void Reciveknockback(Vector2 knockback, float duration)
        // {
        //     if (KnockbackCo != null)
        //     {
        //         StopCoroutine(KnockbackCo);
        //     }
        //     KnockbackCo = StartCoroutine(konckbackCo(knockback, duration));
        // }
        // private IEnumerator konckbackCo(Vector2 knockback ,float duration)
        // {
        //     isknocked = true;
        //     rb.linearVelocity = knockback;
        //     yield return new WaitForSeconds(duration);
        //     rb.linearVelocity = Vector2.zero;
        //     isknocked = false;
        // }


        protected override void OnDrawGizmos()
        {
            
            base.OnDrawGizmos();
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(playerCheck.position,
                new Vector3(playerCheck.position.x + (FacingDirection * playerCheckDistance), playerCheck.position.y));

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(playerCheck.position,
                new Vector3(playerCheck.position.x + (FacingDirection * attackDistance), playerCheck.position.y));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(playerCheck.position,
                new Vector3(playerCheck.position.x + (FacingDirection * minRetreatDistance), playerCheck.position.y));
        }


    }
