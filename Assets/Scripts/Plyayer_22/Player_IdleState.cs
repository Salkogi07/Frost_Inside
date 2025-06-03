namespace Script.Plyayer_22
{
    public class Player_IdleState : Player_GroundedState
    {
        public Player_IdleState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
        {
        }

        public override void Enter()
        {
            base.Enter();

            player.SetVelocity(0, rigidbody.linearVelocity.y);
        }

        public override void Update()
        {
            base.Update();

            if (player.MoveInput != 0)
                stateMachine.ChangeState(player.WalkState);
        }
    }
}
