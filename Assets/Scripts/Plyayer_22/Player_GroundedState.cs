using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_GroundedState : EntityState
    {
        public Player_GroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
        {
        }

        public override void Update()
        {
            base.Update();

            Debug.Log("ground");
            Debug.Log(rb.linearVelocity.y);

            if (rb.linearVelocity.y < 0)
                stateMachine.ChangeState(player.FallState);

            if (input.Player.Jump.WasPerformedThisFrame())
                stateMachine.ChangeState(player.JumpState);
        }
    }
}
