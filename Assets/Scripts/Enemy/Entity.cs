using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{

    public Animator Anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
        
    protected Enemy_StateMachine EnemyStateMachine;
    


        
        // public float lastTimeWasInBattle;
        // public float inGameTime;
        
        
        
        private bool _isFacingRight = false;
        public int FacingDirection { get; private set; } = -1;

        [Header("Collision detection")]
        [SerializeField] protected LayerMask whatIsGround;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckDistance;
        [SerializeField] private Transform primaryWallCheck;
        [SerializeField] private Transform secondaryWallCheck;
        [SerializeField] private LayerMask whatIsWall;
        [SerializeField] private float wallCheckDistance;
    
        

        // Enemy 코드를 따로 만들어야함 EntityDeath() 
        // ovrrive
        
        // public void TryEnterBattleState(Transform player)
        // {
        //     if (EnemyStateMachine.currentState == BattleState)
        //     {
        //         return;
        //     }
        //     if(EnemyStateMachine.currentState == AttackState)
        //     {
        //         return;
        //     }
        //     this.player =  player;
        //     EnemyStateMachine.ChangeState(BattleState);
        // }

        // public Transform GetPlayerReference()
        // {
        //     if (player == null)
        //     {
        //         player = PlayerDetection().transform;            
        //     }
        //     return player;
        // }
    
        
        public bool IsGroundDetected { get; private set; }
        public bool IsWallDetected { get; private set; }
        

        private Coroutine KnockbackCo;
        private bool isknocked;
        protected virtual void Awake()
        {
            Anim = GetComponentInChildren<Animator>();
            rb = GetComponent<Rigidbody2D>();

            EnemyStateMachine = new Enemy_StateMachine();
            // Debug.Log("dasjkdhaskj");
            
        }
        

        protected virtual void Start()
        {
            
        }

        //E
        protected virtual void Update()
        {
            // inGameTime += Time.deltaTime;
            EnemyStateMachine.UpdateActiveState();
            // Debug.Log("34");
        }

        protected virtual void FixedUpdate()
        {
            HandleCollisionDetection();
            EnemyStateMachine.FiexedUpdateActiveState();
            // Debug.Log("F34");
        }

        public virtual void EntityDeath()
        {
            
        }
        //E
        public void Reciveknockback(Vector2 knockback, float duration)
        {
            if (KnockbackCo != null)
            {
                StopCoroutine(KnockbackCo);
            }
            KnockbackCo = StartCoroutine(konckbackCo(knockback, duration));
        }
        
        
        private IEnumerator konckbackCo(Vector2 knockback ,float duration)
        {
            isknocked = true;
            rb.linearVelocity = knockback;
            yield return new WaitForSeconds(duration);
            rb.linearVelocity = Vector2.zero;
            isknocked = false;
        }
        
        //E
        public void CallAnimationTrigger()
        {
            EnemyStateMachine.currentState.CallAnimationTrigger();
        }
        
        
        //E
        public void SetVelocity(float xVelocity, float yVelocity)
        {
            if (isknocked)
            {
                return;
            }
            
            rb.linearVelocity = new Vector2(xVelocity, yVelocity);
            HandleFlip(xVelocity);
        }

        // E
        public void HandleFlip(float xVelcoity)
        {
            if (xVelcoity > 0 && !_isFacingRight)
                Flip();
            else if (xVelcoity < 0 && _isFacingRight)
                Flip();
        }
        //E
        public void Flip()
        {
            transform.Rotate(0, 180, 0);
            _isFacingRight = !_isFacingRight;
            FacingDirection *= -1;
        }

        //E
        private void HandleCollisionDetection()
        {
            IsGroundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
            IsWallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, whatIsWall) 
                             && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, whatIsWall);
        }

        //E
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance));
            Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * FacingDirection, 0));
            Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * FacingDirection, 0));
            
            
        }
}
