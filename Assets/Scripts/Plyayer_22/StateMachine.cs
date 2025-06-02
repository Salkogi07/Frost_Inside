namespace Script.Plyayer_22
{
    public class StateMachine
    {
        public EntityState currentState { get; private set; }

        public void Initialize(EntityState startState)
        {
            currentState = startState;
            currentState.Enter();
        }

        public void ChangeState(EntityState newState)
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
