namespace Scripts.Enemy
{
    public class Enemy_Skeleton : Enemy
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            
            EnemyStateMachine.Initialize(IdleState);
        }
    }
}