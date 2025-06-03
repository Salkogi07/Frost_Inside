using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_WalkState : Player_GroundedState
    {
        public Player_WalkState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
        {
        }

        public override void Update()
        {
            base.Update();
            
            if (player.MoveInput == 0)
                stateMachine.ChangeState(player.IdleState);
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();

            player.SetMoveSpeed(player.WalkSpeed);
            player.SetVelocity(player.MoveInput * player.CurrentSpeed, rigidbody.linearVelocity.y);
        }
    }
}
