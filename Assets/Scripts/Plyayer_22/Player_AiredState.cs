using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player_AiredState : PlayerState
    {
        public Player_AiredState(Player player, Player_StateMachine playerStateMachine, string animBoolName) : base(player, playerStateMachine, animBoolName)
        {
        }

        public override void Update()
        {
            base.Update();

            if (player.MoveInput != 0)
                player.SetVelocity(player.MoveInput * (player.CurrentSpeed * player.inAirMoveMultiplier) , rigidbody.linearVelocity.y);
        }
    }
}
