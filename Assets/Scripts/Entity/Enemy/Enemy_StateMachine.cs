    //
    // public class Enemy_StateMachine
    // {
    //     public EnemyState currentState { get; private set; }
    //     public bool canChangeState;
    //
    //     public void Initialize(EnemyState startState)
    //     {
    //         canChangeState = true;
    //         currentState = startState;
    //         currentState.Enter();
    //     }
    //
    //     public void ChangeState(EnemyState newState)
    //     {
    //         if (canChangeState == false)
    //         {
    //             return;
    //         }
    //         
    //         currentState.Exit();
    //         currentState = newState;
    //         currentState.Enter();
    //     }
    //
    //     
    //     public void UpdateActiveState()
    //     {
    //         currentState.Update();
    //     }
    //
    //     public void FiexedUpdateActiveState()
    //     {
    //         currentState.FiexedUpdate();
    //     }
    //     
    //     public void SwitchOffStateMachine() => canChangeState = false;
    // }
