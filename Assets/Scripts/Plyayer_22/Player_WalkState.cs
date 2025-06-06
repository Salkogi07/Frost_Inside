using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_WalkState : Player_GroundedState
    {
        public Player_WalkState(Player player, Player_StateMachine playerStateMachine, string stateName) : base(player, playerStateMachine, stateName)
        {
        }

        public override void Update()
        {
            base.Update();
            
            player.Condition.StaminaRecovery();
            
            if (player.MoveInput == 0)
                playerStateMachine.ChangeState(player.IdleState);
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();

            player.SetMoveSpeed(player.Stats.WalkSpeed);
            player.SetVelocity(player.MoveInput * player.CurrentSpeed, rigidbody.linearVelocity.y);
        }
    }
}
