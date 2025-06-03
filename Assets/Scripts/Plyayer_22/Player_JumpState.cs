namespace Script.Plyayer_22
{
    public class Player_JumpState : Player_AiredState
    {
        public Player_JumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();

            player.SetVelocity(rigidbody.linearVelocity.x, player.JumpForce);
        }

        public override void Update()
        {
            base.Update();

            if (rigidbody.linearVelocity.y < 0)
                stateMachine.ChangeState(player.FallState);
        }
    }
}
