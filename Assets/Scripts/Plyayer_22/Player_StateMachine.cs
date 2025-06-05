namespace Script.Plyayer_22
{
    public class Player_StateMachine
    {
        public PlayerState currentState { get; private set; }

        public void Initialize(PlayerState startState)
        {
            currentState = startState;
            currentState.Enter();
        }

        public void ChangeState(PlayerState newState)
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
