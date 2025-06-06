namespace Script.Plyayer_22
{
    public class Player_JumpState : Player_AiredState
    {
        public Player_JumpState(Player player, Player_StateMachine playerStateMachine, string animBoolName) : base(player, playerStateMachine, animBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            player.Condition.UseStaminaToJump();
            player.SetVelocity(rigidbody.linearVelocity.x, player.JumpForce);
        }

        public override void Update()
        {
            base.Update();

            if (rigidbody.linearVelocity.y < 0)
                playerStateMachine.ChangeState(player.FallState);
        }
    }
}
