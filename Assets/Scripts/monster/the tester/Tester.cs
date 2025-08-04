


    public class Tester : Enemy
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

