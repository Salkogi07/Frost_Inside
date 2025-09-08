
    public class Enemy_snow_wolf_StateMachine
    {
        public Enemy_snow_wolf_State currentState { get; private set; }
        public bool canChangeState;

        public void Initialize(Enemy_snow_wolf_State startState)
        {
            canChangeState = true;
            currentState = startState;
            currentState.Enter();
        }

        public void ChangeState(Enemy_snow_wolf_State newState)
        {
            if (canChangeState == false)
            {
                return;
            }
            
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
        
        public void SwitchOffStateMachine() => canChangeState = false;
    }
