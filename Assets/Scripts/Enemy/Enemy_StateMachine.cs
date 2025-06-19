namespace Scripts.Enemy
{
    public class Enemy_StateMachine
    {
        public EnemyState currentState { get; private set; }

        public void Initialize(EnemyState startState)
        {
            currentState = startState;
            currentState.Enter();
        }

        public void ChangeState(EnemyState newState)
        {
            currentState.Exit();
            currentState = newState;
            currentState.Enter();
        }

        
        public void UpdateActiveState()
        {
            currentState.Update();
        }

        public void FiexedUpdateActiveState()
        {
            currentState.FiexedUpdate();
        }
    }
}